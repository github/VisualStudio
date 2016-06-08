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

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
        [ImportingConstructor]
        PullRequestCreationViewModel(
             IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice)
             : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo)
         {}

        public PullRequestCreationViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel activeRepo)
        {
            repositoryHost.ModelService.GetBranches(activeRepo)
                            .ToReadOnlyList()
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(x => Branches = x);

            var repo = GitService.GitServiceHelper.GetRepo(activeRepo.LocalPath);
            SourceBranch = new BranchModel(repo.Head);

            // what do we do if there's no master?
            TargetBranch = new BranchModel { Name = "master" };
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
    }
}
