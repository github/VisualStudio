using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class ForkRepositorySelectViewModelDesigner : ViewModelBase, IForkRepositorySelectViewModel
    {
        public ForkRepositorySelectViewModelDesigner()
        {
            Accounts = new[]
            {
                new AccountDesigner { Login = "Myself" },
                new AccountDesigner { Login = "MyOrg1" },
                new AccountDesigner { Login = "MyOrg2" },
                new AccountDesigner { Login = "MyOrg3" },
                new AccountDesigner { Login = "a-long-org-name" },
            };

            ExistingForks = new[]
            {
                new RemoteRepositoryModelDesigner { Owner = "MyOrg5", Name = "MyRepo" },
                new RemoteRepositoryModelDesigner { Owner = "MyOrg6", Name = "MyRepo" },
            };
        }

        public IReadOnlyList<IAccount> Accounts { get; set; }

        public IObservable<object> Done => null;

        public IReadOnlyList<IRemoteRepositoryModel> ExistingForks { get; set; }

        public bool IsLoading { get; set; }

        public string Title => null;

        public ReactiveCommand<object> SelectedAccount => null;

        public ReactiveCommand<object> CloneRepository => null;

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            return Task.CompletedTask;
        }
    }
}