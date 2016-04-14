using GitHub.Exports;
using GitHub.Models;
using NullGuard;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.ViewModels
{
    //Add properties to interface first
    //Ex would be login tab view model
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
        public PullRequestCreationViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel repository)
        {

        }

       public IReadOnlyList<IBranch> Branches { get; }

        public IBranch CurrentBranch { get; private set; }

        public IAccount SelectedAssignee {get; private set;}

        public IBranch TargetBranch { get; private set; }

        public IReadOnlyList<IAccount> Users
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
