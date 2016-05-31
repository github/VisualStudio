using System.Diagnostics.CodeAnalysis;
using GitHub.ViewModels;
using System.Collections.Generic;
using ReactiveUI;
using System.Collections.ObjectModel;
using GitHub.Models;
using System;
using System.Windows.Media.Imaging;
using GitHub.Services;
using System.Reactive;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestCreationViewModelDesigner : BaseViewModelDesigner, IPullRequestCreationViewModel
    {
        public PullRequestCreationViewModelDesigner()
        {

            CurrentBranch = new BranchModel { Name = "fix-everything" };

            SelectedAssignee = new AccountDesigner { Login = "Haacked (Phil Haack)" }; 

            TargetBranch = new BranchModel { Name = "master" };

        }

        public IAccount SelectedAssignee { get; set; }
        public IBranchModel TargetBranch { get; set; }
        public IBranchModel CurrentBranch { get; set; }
        public IReadOnlyList<IBranchModel> Branches { get; set; }
        public IReadOnlyList<IAccount> Assignees { get; set; }
        public IReactiveCommand<Unit> CreatePullRequest { get; set; }


    }
}