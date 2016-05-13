using System.Diagnostics.CodeAnalysis;
using GitHub.ViewModels;
using System.Collections.Generic;
using ReactiveUI;
using System.Collections.ObjectModel;
using GitHub.Models;
using System;
using System.Windows.Media.Imaging;
using GitHub.Services;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestCreationViewModelDesigner : BaseViewModelDesigner, IPullRequestCreationViewModel
    {
        public PullRequestCreationViewModelDesigner()
        {
            
            CurrentBranchName = "fix-everything" ;

            SelectedAssignee = new AccountDesigner { Login = "Haacked (Phil Haack)" }; 

            TargetBranch = new BranchModel { Name = "master" };

            Assignees = new ObservableCollection<IAccount>
            {
                new AccountDesigner { Login = "Haacked (Phil Haack)" },
                new AccountDesigner { Login = "shana(Andreia Gaita)" }            
            };
        }

        public IAccount SelectedAssignee { get; set; }
        public IBranchModel TargetBranch { get; set; }
        public string CurrentBranchName { get; set; }
        public IReadOnlyList<IBranchModel> Branches { get; set; }
        public ObservableCollection<IAccount> Assignees { get; set; }




    }
}