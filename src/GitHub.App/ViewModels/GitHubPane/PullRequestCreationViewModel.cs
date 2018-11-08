using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Models.Drafts;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Validation;
using Octokit;
using ReactiveUI;
using Serilog;
using IConnection = GitHub.Models.IConnection;
using static System.FormattableString;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestCreationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class PullRequestCreationViewModel : PanePageViewModelBase, IPullRequestCreationViewModel
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestCreationViewModel>();

        readonly ObservableAsPropertyHelper<bool> isExecuting;
        readonly IPullRequestService service;
        readonly IModelServiceFactory modelServiceFactory;
        readonly IMessageDraftStore draftStore;
        readonly IGitService gitService;
        readonly IScheduler timerScheduler;
        readonly CompositeDisposable disposables = new CompositeDisposable();
        LocalRepositoryModel activeLocalRepo;
        ObservableAsPropertyHelper<RemoteRepositoryModel> githubRepository;
        IModelService modelService;

        [ImportingConstructor]
        public PullRequestCreationViewModel(
            IModelServiceFactory modelServiceFactory,
            IPullRequestService service,
            INotificationService notifications,
            IMessageDraftStore draftStore,
            IGitService gitService)
            : this(modelServiceFactory, service, notifications, draftStore, gitService, DefaultScheduler.Instance)
        {
        }

        public PullRequestCreationViewModel(
            IModelServiceFactory modelServiceFactory,
            IPullRequestService service,
            INotificationService notifications,
            IMessageDraftStore draftStore,
            IGitService gitService,
            IScheduler timerScheduler)
        {
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(notifications, nameof(notifications));
            Guard.ArgumentNotNull(draftStore, nameof(draftStore));
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(timerScheduler, nameof(timerScheduler));

            this.service = service;
            this.modelServiceFactory = modelServiceFactory;
            this.draftStore = draftStore;
            this.gitService = gitService;
            this.timerScheduler = timerScheduler;

            this.WhenAnyValue(x => x.Branches)
                .WhereNotNull()
                .Where(_ => TargetBranch != null)
                .Subscribe(x =>
                {
                    if (!x.Any(t => t.Equals(TargetBranch)))
                    {
                        TargetBranch = GitHubRepository.IsFork ? GitHubRepository.Parent.DefaultBranch : GitHubRepository.DefaultBranch;
                    }
                });

            SetupValidators();

            var whenAnyValidationResultChanges = this.WhenAny(
                x => x.TitleValidator.ValidationResult,
                x => x.BranchValidator.ValidationResult,
                x => x.IsBusy,
                (x, y, z) => (x.Value?.IsValid ?? false) && (y.Value?.IsValid ?? false) && !z.Value);

            this.WhenAny(x => x.BranchValidator.ValidationResult, x => x.GetValue())
                .WhereNotNull()
                .Where(x => !x.IsValid && x.DisplayValidationError)
                .Subscribe(x => notifications.ShowError(BranchValidator.ValidationResult.Message));

            CreatePullRequest = ReactiveCommand.CreateFromObservable(
                () => service
                    .CreatePullRequest(modelService, activeLocalRepo, TargetBranch.Repository, SourceBranch, TargetBranch, PRTitle, Description ?? String.Empty)
                    .Catch<IPullRequestModel, Exception>(ex =>
                    {
                        log.Error(ex, "Error creating pull request");

                        //TODO:Will need a uniform solution to HTTP exception message handling
                        var apiException = ex as ApiValidationException;
                        var error = apiException?.ApiError?.Errors?.FirstOrDefault();
                        notifications.ShowError(error?.Message ?? ex.Message);
                        return Observable.Empty<IPullRequestModel>();
                    }),
                whenAnyValidationResultChanges);
            CreatePullRequest.Subscribe(pr =>
            {
                notifications.ShowMessage(String.Format(CultureInfo.CurrentCulture, Resources.PRCreatedUpstream, SourceBranch.DisplayName, TargetBranch.Repository.Owner + "/" + TargetBranch.Repository.Name + "#" + pr.Number,
                    TargetBranch.Repository.CloneUrl.ToRepositoryUrl().Append("pull/" + pr.Number)));
                NavigateTo("/pulls?refresh=true");
                Cancel.Execute();
                draftStore.DeleteDraft(GetDraftKey(), string.Empty).Forget();
                Close();
            });

            Cancel = ReactiveCommand.Create(() => { });
            Cancel.Subscribe(_ =>
            {
                Close();
                draftStore.DeleteDraft(GetDraftKey(), string.Empty).Forget();
            });

            isExecuting = CreatePullRequest.IsExecuting.ToProperty(this, x => x.IsExecuting);

            this.WhenAnyValue(x => x.Initialized, x => x.GitHubRepository, x => x.IsExecuting)
                .Select(x => !(x.Item1 && x.Item2 != null && !x.Item3))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => IsBusy = x);
        }

        public async Task InitializeAsync(LocalRepositoryModel repository, IConnection connection)
        {
            modelService = await modelServiceFactory.CreateAsync(connection);
            activeLocalRepo = repository;
            SourceBranch = gitService.GetBranch(repository);

            var obs = modelService.ApiClient.GetRepository(repository.Owner, repository.Name)
                .Select(r => CreateRemoteRepositoryModel(r))
                .PublishLast();
            disposables.Add(obs.Connect());
            var githubObs = obs;

            githubRepository = githubObs.ToProperty(this, x => x.GitHubRepository);

            this.WhenAnyValue(x => x.GitHubRepository)
                .WhereNotNull()
                .Subscribe(r =>
                {
                    TargetBranch = r.IsFork ? r.Parent.DefaultBranch : r.DefaultBranch;
                });

            githubObs.SelectMany(r =>
            {
                var b = Observable.Empty<BranchModel>();
                if (r.IsFork)
                {
                    b = modelService.GetBranches(r.Parent).Select(x =>
                    {
                        return x;
                    });
                }
                return b.Concat(modelService.GetBranches(r));
            })
            .ToList()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x =>
            {
                Branches = x.ToList();
                Initialized = true;
            });

            var draftKey = GetDraftKey();
            await LoadInitialState(draftKey).ConfigureAwait(true);

            this.WhenAnyValue(
                x => x.PRTitle,
                x => x.Description,
                (t, d) => new PullRequestDraft { Title = t, Body = d })
                .Throttle(TimeSpan.FromSeconds(1), timerScheduler)
                .Subscribe(x => draftStore.UpdateDraft(draftKey, string.Empty, x));

            Initialized = true;
        }

        static RemoteRepositoryModel CreateRemoteRepositoryModel(Repository repository)
        {
            var ownerAccount = new Models.Account(repository.Owner);
            var parent = repository.Parent != null ? CreateRemoteRepositoryModel(repository.Parent) : null;
            var model = new RemoteRepositoryModel(repository.Id, repository.Name, repository.CloneUrl,
                repository.Private, repository.Fork, ownerAccount, parent, repository.DefaultBranch);

            if (parent != null)
            {
                parent.DefaultBranch.DisplayName = parent.DefaultBranch.Id;
            }

            return model;
        }

        async Task LoadInitialState(string draftKey)
        {
            if (activeLocalRepo.CloneUrl == null)
                return;

            var draft = await draftStore.GetDraft<PullRequestDraft>(draftKey, string.Empty).ConfigureAwait(true);

            if (draft != null)
            {
                PRTitle = draft.Title;
                Description = draft.Body;
            }
            else
            {
                LoadDescriptionFromCommits();
            }
        }

        void LoadDescriptionFromCommits()
        {
            SourceBranch = gitService.GetBranch(activeLocalRepo);

            var uniqueCommits = this.WhenAnyValue(
                x => x.SourceBranch,
                x => x.TargetBranch)
                .Where(x => x.Item1 != null && x.Item2 != null)
                .Select(branches =>
                {
                    var baseBranch = branches.Item1.Name;
                    var compareBranch = branches.Item2.Name;

                    // We only need to get max two commits for what we're trying to achieve here.
                    // If there's no commits we want to block creation of the PR, if there's one commits
                    // we wan't to use its commit message as the PR title/body and finally if there's more
                    // than one we'll use the branch name for the title.
                    return service.GetMessagesForUniqueCommits(activeLocalRepo, baseBranch, compareBranch, maxCommits: 2)
                        .Catch<IReadOnlyList<CommitMessage>, Exception>(ex =>
                        {
                            log.Warning(ex, "Could not load unique commits");
                            return Observable.Empty<IReadOnlyList<CommitMessage>>();
                        });
                })
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Replay(1)
                .RefCount();

            Observable.CombineLatest(
                this.WhenAnyValue(x => x.SourceBranch),
                uniqueCommits,
                service.GetPullRequestTemplate(activeLocalRepo).DefaultIfEmpty(string.Empty),
                (compare, commits, template) => new { compare, commits, template })
                .Subscribe(x =>
                {
                    var prTitle = string.Empty;
                    var prDescription = string.Empty;

                    if (x.commits.Count == 1)
                    {
                        prTitle = x.commits[0].Summary;
                        prDescription = x.commits[0].Details;
                    }
                    else
                    {
                        prTitle = x.compare.Name.Humanize();
                    }

                    if (!string.IsNullOrWhiteSpace(x.template))
                    {
                        if (!string.IsNullOrEmpty(prDescription))
                            prDescription += "\n\n";
                        prDescription += x.template;
                    }

                    PRTitle = prTitle;
                    Description = prDescription;
                });
        }

        void SetupValidators()
        {
            var titleObs = this.WhenAnyValue(x => x.PRTitle);
            TitleValidator = ReactivePropertyValidator.ForObservable(titleObs)
                .IfNullOrEmpty(Resources.PullRequestCreationTitleValidatorEmpty);

            var branchObs = this.WhenAnyValue(
                    x => x.Initialized,
                    x => x.TargetBranch,
                    x => x.SourceBranch,
                    (init, target, source) => new { Initialized = init, Source = source, Target = target })
                .Where(x => x.Initialized);

            BranchValidator = ReactivePropertyValidator.ForObservable(branchObs)
                .IfTrue(x => x.Source == null, Resources.PullRequestSourceBranchDoesNotExist)
                .IfTrue(x => x.Source.Equals(x.Target), Resources.PullRequestSourceAndTargetBranchTheSame);
        }

        bool disposed; // To detect redundant calls
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (disposed) return;
                disposed = true;

                disposables.Dispose();
            }
        }

        public static string GetDraftKey(
            UriString cloneUri,
            string branchName)
        {
            return Invariant($"pr|{cloneUri}|{branchName}");
        }

        protected string GetDraftKey()
        {
            return GetDraftKey(
                activeLocalRepo.CloneUrl,
                SourceBranch.Name);
        }

        public RemoteRepositoryModel GitHubRepository { get { return githubRepository?.Value; } }
        bool IsExecuting { get { return isExecuting.Value; } }

        bool initialized;
        bool Initialized
        {
            get { return initialized; }
            set { this.RaiseAndSetIfChanged(ref initialized, value); }
        }

        BranchModel sourceBranch;
        public BranchModel SourceBranch
        {
            get { return sourceBranch; }
            set { this.RaiseAndSetIfChanged(ref sourceBranch, value); }
        }

        BranchModel targetBranch;
        public BranchModel TargetBranch
        {
            get { return targetBranch; }
            set { this.RaiseAndSetIfChanged(ref targetBranch, value); }
        }

        IReadOnlyList<BranchModel> branches;
        public IReadOnlyList<BranchModel> Branches
        {
            get { return branches; }
            set { this.RaiseAndSetIfChanged(ref branches, value); }
        }

        public ReactiveCommand<Unit, IPullRequestModel> CreatePullRequest { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        string title;
        public string PRTitle
        {
            get { return title; }
            set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        string description;
        public string Description
        {
            get { return description; }
            set { this.RaiseAndSetIfChanged(ref description, value); }
        }

        ReactivePropertyValidator titleValidator;
        public ReactivePropertyValidator TitleValidator
        {
            get { return titleValidator; }
            set { this.RaiseAndSetIfChanged(ref titleValidator, value); }
        }

        ReactivePropertyValidator branchValidator;
        ReactivePropertyValidator BranchValidator
        {
            get { return branchValidator; }
            set { this.RaiseAndSetIfChanged(ref branchValidator, value); }
        }
    }
}
