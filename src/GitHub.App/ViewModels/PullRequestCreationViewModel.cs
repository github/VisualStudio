using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Models;
using System.Collections.Generic;
using ReactiveUI;
using GitHub.Services;
using System.Reactive.Linq;
using GitHub.Extensions.Reactive;
using GitHub.UI;
using System.Linq;
using GitHub.Validation;
using GitHub.App;
using System.Diagnostics.CodeAnalysis;
using Octokit;
using NLog;
using System.Globalization;
using GitHub.Extensions;
using System.Reactive.Disposables;
using System.Reactive;
using IConnection = GitHub.Models.IConnection;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class PullRequestCreationViewModel : DialogViewModelBase, IPullRequestCreationViewModel, IDisposable
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly ObservableAsPropertyHelper<IRemoteRepositoryModel> githubRepository;
        readonly ObservableAsPropertyHelper<bool> isExecuting;
        readonly IConnection connection;
        readonly IModelService modelService;
        readonly IObservable<IRemoteRepositoryModel> githubObs;
        readonly CompositeDisposable disposables = new CompositeDisposable();
        readonly ILocalRepositoryModel activeLocalRepo;

        [ImportingConstructor]
        PullRequestCreationViewModel(
            IConnection connection,
            ITeamExplorerServiceHolder teservice,
            IPullRequestService service, INotificationService notifications)
            : this(connection, teservice?.ActiveRepo, service, notifications)
        {}

        public PullRequestCreationViewModel(IConnection connection, ILocalRepositoryModel activeRepo,
            IPullRequestService service, INotificationService notifications)
        {
            Extensions.Guard.ArgumentNotNull(connection, nameof(connection));
            Extensions.Guard.ArgumentNotNull(activeRepo, nameof(activeRepo));
            Extensions.Guard.ArgumentNotNull(service, nameof(service));
            Extensions.Guard.ArgumentNotNull(notifications, nameof(notifications));

            this.connection = connection;
            activeLocalRepo = activeRepo;

            var obs = modelService.ApiClient.GetRepository(activeRepo.Owner, activeRepo.Name)
                .Select(r => new RemoteRepositoryModel(r))
                .PublishLast();
            disposables.Add(obs.Connect());
            githubObs = obs;

            githubRepository = githubObs.ToProperty(this, x => x.GitHubRepository);

            this.WhenAnyValue(x => x.GitHubRepository)
                .WhereNotNull()
                .Subscribe(r =>
            {
                TargetBranch = r.IsFork ? r.Parent.DefaultBranch : r.DefaultBranch;
            });

            SourceBranch = activeRepo.CurrentBranch;
            service.GetPullRequestTemplate(activeRepo)
                .Subscribe(x => Description = x ?? String.Empty, () => Description = Description ?? String.Empty);

            this.WhenAnyValue(x => x.Branches)
                .WhereNotNull()
                .Where(_ => TargetBranch != null)
                .Subscribe(x =>
                {
                    if (!x.Any(t => t.Equals(TargetBranch)))
                        TargetBranch = GitHubRepository.IsFork ? GitHubRepository.Parent.DefaultBranch : GitHubRepository.DefaultBranch;
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

            CreatePullRequest = ReactiveCommand.CreateAsyncObservable(whenAnyValidationResultChanges,
                _ => service
                    .CreatePullRequest(connection, activeRepo, TargetBranch.Repository, SourceBranch, TargetBranch, PRTitle, Description ?? String.Empty)
                    .Catch<IPullRequestModel, Exception>(ex =>
                    {
                        log.Error(ex);

                        //TODO:Will need a uniform solution to HTTP exception message handling
                        var apiException = ex as ApiValidationException;
                        var error = apiException?.ApiError?.Errors?.FirstOrDefault();
                        notifications.ShowError(error?.Message ?? ex.Message);
                        return Observable.Empty<IPullRequestModel>();
                    }))
            .OnExecuteCompleted(pr =>
            {
                notifications.ShowMessage(String.Format(CultureInfo.CurrentCulture, Resources.PRCreatedUpstream, SourceBranch.DisplayName, TargetBranch.Repository.Owner + "/" + TargetBranch.Repository.Name + "#" + pr.Number,
                    TargetBranch.Repository.CloneUrl.ToRepositoryUrl().Append("pull/" + pr.Number)));
            });

            isExecuting = CreatePullRequest.IsExecuting.ToProperty(this, x => x.IsExecuting);

            this.WhenAnyValue(x => x.Initialized, x => x.GitHubRepository, x => x.Description, x => x.IsExecuting)
                .Select(x => !(x.Item1 && x.Item2 != null && x.Item3 != null && !x.Item4))
                .Subscribe(x => IsBusy = x);
        }

        public override void Initialize(ViewWithData data = null)
        {
            base.Initialize(data);

            Initialized = false;

            githubObs.SelectMany(r =>
            {
                var b = Observable.Empty<IBranch>();
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

            SourceBranch = activeLocalRepo.CurrentBranch;
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
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;
                disposed = true;

                disposables.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IRemoteRepositoryModel GitHubRepository { get { return githubRepository?.Value; } }
        bool IsExecuting { get { return isExecuting.Value; } }

        bool initialized;
        bool Initialized
        {
            get { return initialized; }
            set { this.RaiseAndSetIfChanged(ref initialized, value); }
        }

        IBranch sourceBranch;
        public IBranch SourceBranch
        {
            get { return sourceBranch; }
            set { this.RaiseAndSetIfChanged(ref sourceBranch, value); }
        }

        IBranch targetBranch;
        public IBranch TargetBranch
        {
            get { return targetBranch; }
            set { this.RaiseAndSetIfChanged(ref targetBranch, value); }
        }

        IReadOnlyList<IBranch> branches;
        public IReadOnlyList<IBranch> Branches
        {
            get { return branches; }
            set { this.RaiseAndSetIfChanged(ref branches, value); }
        }

        public IReactiveCommand<IPullRequestModel> CreatePullRequest { get; }

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

        public override IObservable<Unit> Done => CreatePullRequest.SelectUnit();
    }
}
