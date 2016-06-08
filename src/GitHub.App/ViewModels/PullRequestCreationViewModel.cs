using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Models;
using System.Collections.Generic;
using ReactiveUI;
using GitHub.Services;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using GitHub.Extensions.Reactive;
using GitHub.UI;
using System.Linq;
using System.Windows.Input;
using GitHub.Validation;
using GitHub.Extensions;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
        [ImportingConstructor]
        PullRequestCreationViewModel(
             IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice,
             IPullRequestService service)
             : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo, service)
         {}

        public PullRequestCreationViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel activeRepo, IPullRequestService service)
        {
            repositoryHost.ModelService.GetBranches(activeRepo)
                            .ToReadOnlyList()
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(x => Branches = x);

            var repo = GitService.GitServiceHelper.GetRepo(activeRepo.LocalPath);
            SourceBranch = new BranchModel(repo.Head);

            // what do we do if there's no master?
            TargetBranch = new BranchModel { Name = "master" };
            var titleObs = this.WhenAny(x => x.PRTitle, x => x.Value).WhereNotNull();
            TitleValidator = ReactivePropertyValidator.ForObservable(titleObs)
                .IfNullOrEmpty("Please enter a title for the Pull Request");

            var branchObs = this.WhenAny(
                x => x.SourceBranch,
                x => x.TargetBranch,
                (source, target) => source.Value.Name == target.Value.Name);

            BranchValidator = ReactivePropertyValidator.ForObservable(branchObs)
                .IfTrue(x => x, "Source and target branch cannot be the same");

            var whenAnyValidationResultChanges = this.WhenAny(
                x => x.TitleValidator.ValidationResult.IsValid,
                x => x.BranchValidator.ValidationResult.IsValid,
                (x, y) => x.Value && y.Value);

            createPullRequest = ReactiveCommand.CreateAsyncObservable(whenAnyValidationResultChanges,
                _ => service.CreatePullRequest(repositoryHost, activeRepo, PRTitle, SourceBranch, TargetBranch)
            );
            createPullRequest.ThrownExceptions.Subscribe(ex =>
            {
                if (!ex.IsCriticalException())
                {
                }
            });
        }

        IBranch targetBranch;
        public IBranch TargetBranch
        {
            get { return targetBranch; }
            set { this.RaiseAndSetIfChanged(ref targetBranch, value); }
        }

        IBranch sourceBranch;
        public IBranch SourceBranch
        {
            get { return sourceBranch; }
            set { this.RaiseAndSetIfChanged(ref sourceBranch, value); }
        }

        IReadOnlyList<IBranch> branches;
        public IReadOnlyList<IBranch> Branches
        {
            get { return branches; }
            set { this.RaiseAndSetIfChanged(ref branches, value); }
        }

        IReactiveCommand createPullRequest;
        public ICommand CreatePullRequest => createPullRequest;

        string title;
        public string PRTitle
        {
            get { return title; }
            set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        ReactivePropertyValidator titleValidator;
        public ReactivePropertyValidator TitleValidator
        {
            get { return titleValidator; }
            private set { this.RaiseAndSetIfChanged(ref titleValidator, value); }
        }

        ReactivePropertyValidator branchValidator;
        public ReactivePropertyValidator BranchValidator
        {
            get { return branchValidator; }
            private set { this.RaiseAndSetIfChanged(ref branchValidator, value); }
        }
    }
}
