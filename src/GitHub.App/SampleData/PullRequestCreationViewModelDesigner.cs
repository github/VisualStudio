using System.Diagnostics.CodeAnalysis;
using GitHub.ViewModels;
using System.Collections.Generic;
using ReactiveUI;
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
            //TODO:Make String into an IBranch
            Branches = new ReactiveList<IBranch>
            {
               new Branch { Name = "don/stub-ui" },
               new Branch { Name = "feature/pr/views" },
               new Branch { Name = "release-1.0.17.0" }
            };

           
            CurrentBranch = new Branch { Name = "fix-everything" };

            SelectedAssignee = new AccountDesigner { Login = "Haacked (Phil Haack)" }; //IAcct

            TargetBranch = new Branch { Name = "master" }; //IBranch

            //IAcct
            Users = new ReactiveList<IAccount>
            {
                new AccountDesigner { Login = "Haacked (Phil Haack)" },
                new AccountDesigner { Login = "shana(Andreia Gaita)" }            
            };
        }


        public IReactiveList<IBranch> Branches
        {
            get;
            private set;
        }

        public IReadOnlyList<IAccount> Users
        {
            get;
            private set;
        }

        public IAccount SelectedAssignee { get; set; }
        public IBranch TargetBranch { get; set; }
        public IBranch CurrentBranch { get; set; }

    }
}