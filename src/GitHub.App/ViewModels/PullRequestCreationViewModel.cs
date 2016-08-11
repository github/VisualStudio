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
using GitHub.Extensions;
using GitHub.App;
using System.Reactive.Subjects;
using System.Reactive;
using System.Diagnostics.CodeAnalysis;
using Octokit;
using NLog;
using LibGit2Sharp;
using System.Reactive.Threading.Tasks;
using GitHub.Factories;

namespace GitHub.ViewModels
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly ObservableAsPropertyHelper<IRepositoryModel> githubRepository;
        readonly ObservableAsPropertyHelper<bool> isExecuting;
        readonly IRepositoryHost repositoryHost;
        readonly IObservable<IRepositoryModel> githubObs;

        [ImportingConstructor]
        PullRequestCreationViewModel(
             IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice,
             IPullRequestService service, INotificationService notifications)
             : this(connectionRepositoryHostMap?.CurrentRepositoryHost, teservice?.ActiveRepo, service,
                   notifications)
         {}

        public PullRequestCreationViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel activeRepo,
            IPullRequestService service, INotificationService notifications)
        {
            Extensions.Guard.ArgumentNotNull(repositoryHost, nameof(repositoryHost));
            Extensions.Guard.ArgumentNotNull(activeRepo, nameof(activeRepo));
            Extensions.Guard.ArgumentNotNull(service, nameof(service));
            Extensions.Guard.ArgumentNotNull(notifications, nameof(notifications));

            this.repositoryHost = repositoryHost;

            githubObs = repositoryHost.ApiClient.GetRepository(activeRepo.Owner, activeRepo.Name)
                .Select(r => new RepositoryModel(r))
                .PublishLastAndConnect();

            githubRepository = githubObs.ToProperty(this, x => x.GitHubRepository);

            this.WhenAnyValue(x => x.GitHubRepository)
                .WhereNotNull()
                .Subscribe(r =>
            {
                TargetBranch = r.IsFork ? r.Parent.DefaultBranch : r.DefaultBranch;
            });

            SourceBranch = activeRepo.CurrentBranch;
            service.GetPullRequestTemplate(activeRepo)
                .Subscribe(x => Description = x ?? String.Empty);

            this.WhenAnyValue(x => x.Branches)
                .WhereNotNull()
                .Where(_ => TargetBranch != null)
                .Subscribe(x =>
                {
                    if (!x.Any(t => t.Equals(TargetBranch)))
                        TargetBranch = GitHubRepository.IsFork ? GitHubRepository.Parent.DefaultBranch : GitHubRepository.DefaultBranch;
                });

            var titleObs = this.WhenAnyValue(x => x.PRTitle);
            TitleValidator = ReactivePropertyValidator.ForObservable(titleObs)
                .IfNullOrEmpty(Resources.PullRequestCreationTitleValidatorEmpty);

            var branchObs = this.WhenAny(
                    x => x.TargetBranch,
                    x => x.SourceBranch,
                    (target, source) => new { Source = source.Value, Target = target.Value })
                .Where(_ => Initialized)
                .Merge(this.WhenAnyValue(x => x.Initialized).Where(x => x).Select(_ => new { Source = SourceBranch, Target = TargetBranch }));

            BranchValidator = ReactivePropertyValidator.ForObservable(branchObs)
                .IfTrue(x => x.Source == null, Resources.PullRequestSourceBranchDoesNotExist)
                .IfTrue(x => x.Source.Name == x.Target.Name, Resources.PullRequestSourceAndTargetBranchTheSame);

            var whenAnyValidationResultChanges = this.WhenAny(
                x => x.TitleValidator.ValidationResult,
                x => x.BranchValidator.ValidationResult,
                x => x.IsBusy,
                (x, y, z) => (x.Value?.IsValid ?? false) && (y.Value?.IsValid ?? false) && !z.Value);

            this.WhenAny(x => x.BranchValidator.ValidationResult, x => x.GetValue())
                .WhereNotNull()
                .Where(x => !x.IsValid && x.DisplayValidationError)
                .Subscribe(x => notifications.ShowError(BranchValidator.ValidationResult.Message));

            createPullRequest = ReactiveCommand.CreateAsyncObservable(whenAnyValidationResultChanges,
                _ => service
                    .CreatePullRequest(repositoryHost, activeRepo, TargetBranch.Repository, SourceBranch, TargetBranch, PRTitle, Description ?? String.Empty)
                    .Catch<IPullRequestModel, Exception>(ex =>
                    {
                        log.Error(ex);

                        //TODO:Will need a uniform solution to HTTP exception message handling
                        var apiException = ex as ApiValidationException;
                        var error = apiException?.ApiError?.Errors?.FirstOrDefault();
                        notifications.ShowError(error?.Message ?? ex.Message);
                        return Observable.Empty<IPullRequestModel>();
                    }));
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
                var branches = Observable.Empty<IBranch>();
                if (r.IsFork)
                {
                    branches = repositoryHost.ModelService.GetBranches(r.Parent).Select(x =>
                    {
                        x.DisplayName = x.Id;
                        return x;
                    });
                }
                return branches.Concat(repositoryHost.ModelService.GetBranches(r));
            })
            .ToList()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x =>
            {
                BranchesList = new List<IBranch>(x);
                Initialized = true;
            });
        }

        public IRepositoryModel GitHubRepository { get { return githubRepository?.Value; } }
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

        List<IBranch> branchesList;
        List<IBranch> BranchesList
        {
            get { return branchesList; }
            set
            {
                branchesList = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(Branches));
            }
        }

        public IReadOnlyList<IBranch> Branches => branchesList;

        IReactiveCommand<IPullRequestModel> createPullRequest;
        public IReactiveCommand<IPullRequestModel> CreatePullRequest => createPullRequest;

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
