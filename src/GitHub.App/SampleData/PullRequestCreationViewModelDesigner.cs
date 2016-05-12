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
            Branches = new ObservableCollection<IBranch>
            {
               new Branch { Name = "don/stub-ui" },
               new Branch { Name = "feature/pr/views" },
               new Branch { Name = "release-1.0.17.0" }
            };

           
            CurrentBranch = new Branch { Name = "fix-everything" };

            SelectedAssignee = new AccountDesigner { Login = "Haacked (Phil Haack)" }; //IAcct

            TargetBranch = new Branch { Name = "master" }; //IBranch

            //IAcct
            Assignees = new ObservableCollection<IAccount>
            {
                new AccountDesigner { Login = "Haacked (Phil Haack)" },
                new AccountDesigner { Login = "shana(Andreia Gaita)" }            
            };
        }

        public IAccount SelectedAssignee { get; set; }
        public IBranch TargetBranch { get; set; }
        public IBranch CurrentBranch { get; set; }
        public ObservableCollection<IBranch> Branches { get; set; }
        public ObservableCollection<IAccount> Assignees { get; set; }




    }
}