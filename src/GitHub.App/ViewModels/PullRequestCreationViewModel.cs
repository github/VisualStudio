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

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
        readonly IRepositoryHost repositoryHost;
        readonly ISimpleRepositoryModel activeRepo;

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
                .IfNullOrEmpty("Please enter a title for the Pull Request");

            var branchObs = this.WhenAny(
                x => x.SourceBranch,
                source => source.Value);

            BranchValidator = ReactivePropertyValidator.ForObservable(branchObs)
                .IfTrue(x => x == null, "Source branch doesn't exist remotely, have you pushed it?")
                .IfTrue(x => x.Name == TargetBranch.Name, "Source and target branch cannot be the same");

            var whenAnyValidationResultChanges = this.WhenAny(
                x => x.TitleValidator.ValidationResult,
                x => x.BranchValidator.ValidationResult,
                (x, y) => (x.Value?.IsValid ?? false) && (y.Value?.IsValid ?? false));

            this.WhenAny(x => x.BranchValidator.ValidationResult, x => x.GetValue())
                .WhereNotNull()
                .Where(x => !x.IsValid && x.DisplayValidationError)
                .Subscribe(x => notifications.ShowError(BranchValidator.ValidationResult.Message));

            createPullRequest = ReactiveCommand.CreateAsyncObservable(whenAnyValidationResultChanges,
                _ => service.CreatePullRequest(repositoryHost, activeRepo, PRTitle, Description, SourceBranch, TargetBranch)
            );
            createPullRequest.ThrownExceptions.Subscribe(ex =>
            {
                if (!ex.IsCriticalException())
                {
                }
            });
        }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            base.Initialize(data);

            repositoryHost.ModelService.GetBranches(activeRepo)
                            .ToReadOnlyList()
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(x => Branches = x);
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
