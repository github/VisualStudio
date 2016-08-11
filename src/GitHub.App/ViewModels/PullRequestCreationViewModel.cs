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
using NullGuard;
using GitHub.App;
using System.Reactive.Subjects;
using System.Reactive;
using System.Diagnostics.CodeAnalysis;
using Octokit;
using NLog;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IRepositoryHost repositoryHost;
        readonly ISimpleRepositoryModel activeRepo;
        readonly IPullRequestService service;
        readonly Subject<Unit> initializationComplete = new Subject<Unit>();
        bool initialized;

        [ImportingConstructor]
        PullRequestCreationViewModel(
             IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice,
             IPullRequestService service, INotificationService notifications)
             : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo, service, notifications)
         {}

        public PullRequestCreationViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel activeRepo,
            IPullRequestService service, INotificationService notifications)
        {
            this.repositoryHost = repositoryHost;
            this.activeRepo = activeRepo;
            this.service = service;

            var repo = GitService.GitServiceHelper.GetRepo(activeRepo.LocalPath);
            this.WhenAny(x => x.Branches, x => x.Value)
                .WhereNotNull()
                .Subscribe(x =>
                {
                    // what do we do if there's no master?
                    TargetBranch = x.FirstOrDefault(b => b.Name == "master");
                    SourceBranch = x.FirstOrDefault(b => b.Name == repo.Head.FriendlyName);
                });

            var titleObs = this.WhenAny(x => x.PRTitle, x => x.Value);
            TitleValidator = ReactivePropertyValidator.ForObservable(titleObs)
                .IfNullOrEmpty(Resources.PullRequestCreationTitleValidatorEmpty);

            var branchObs = this.WhenAny(
                x => x.TargetBranch,
                x => x.SourceBranch,
                (target, source) => new { Source = source.Value, Target = target.Value })
                .Where(_ => initialized)
                .Merge(initializationComplete.Select(_ => new { Source = SourceBranch, Target = TargetBranch }));

            BranchValidator = ReactivePropertyValidator.ForObservable(branchObs)
                .IfTrue(x => x.Source == null, Resources.PullRequestSourceBranchDoesNotExist)
                .IfTrue(x => x.Source.Name == x.Target.Name, Resources.PullRequestSourceAndTargetBranchTheSame);

            var whenAnyValidationResultChanges = this.WhenAny(
                x => x.TitleValidator.ValidationResult,
                x => x.BranchValidator.ValidationResult,
                (x, y) => (x.Value?.IsValid ?? false) && (y.Value?.IsValid ?? false));

            this.WhenAny(x => x.BranchValidator.ValidationResult, x => x.GetValue())
                .WhereNotNull()
                .Where(x => !x.IsValid && x.DisplayValidationError)
                .Subscribe(x => notifications.ShowError(BranchValidator.ValidationResult.Message));

            createPullRequest = ReactiveCommand.CreateAsyncObservable(whenAnyValidationResultChanges,
                _ => service
                    .CreatePullRequest(repositoryHost, activeRepo, PRTitle, Description ?? String.Empty, SourceBranch, TargetBranch)
                    .Catch<IPullRequestModel, Exception>(ex =>
                    {
                        log.Error(ex);

                        //TODO:Will need a uniform solution to HTTP exception message handling
                        var apiException = ex as ApiValidationException;
                        var error = apiException?.ApiError?.Errors?.FirstOrDefault();
                        notifications.ShowError(error?.Message ?? ex.Message);
                        return Observable.Empty<IPullRequestModel>();
                    }));
        }
        
        public override void Initialize([AllowNull] ViewWithData data)
        {
            initialized = false;
            base.Initialize(data);

            this.Title = null;
            this.Description = service.GetPullRequestTemplate(activeRepo) ?? string.Empty;

            repositoryHost.ModelService.GetBranches(activeRepo)
                            .ToReadOnlyList()
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(x =>
                            {
                                Branches = x;
                                initialized = true;
                                initializationComplete.OnNext(Unit.Default);
                            });
        }

        IBranch sourceBranch;
        [AllowNull]
        public IBranch SourceBranch
        {
            [return: AllowNull]
            get { return sourceBranch; }
            set { this.RaiseAndSetIfChanged(ref sourceBranch, value); }
        }

        IBranch targetBranch;
        [AllowNull]
        public IBranch TargetBranch
        {
            [return: AllowNull]
            get { return targetBranch; }
            set { this.RaiseAndSetIfChanged(ref targetBranch, value); }
        }

        IReadOnlyList<IBranch> branches;
        public IReadOnlyList<IBranch> Branches
        {
            [return: AllowNull]
            get { return branches; }
            set { this.RaiseAndSetIfChanged(ref branches, value); }
        }

        IReactiveCommand<IPullRequestModel> createPullRequest;
        public IReactiveCommand<IPullRequestModel> CreatePullRequest => createPullRequest;

        string title;
        public string PRTitle
        {
            [return: AllowNull]
            get { return title; }
            set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        string description;
        public string Description
        {
            [return: AllowNull]
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
