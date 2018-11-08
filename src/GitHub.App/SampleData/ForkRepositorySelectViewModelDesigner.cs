using System;
using System.Collections.Generic;
using System.Reactive;
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
                new AccountDesigner { Login = "Myself", AvatarUrl = "https://identicons.github.com/myself.png" },
                new AccountDesigner { Login = "MyOrg1", AvatarUrl = "https://identicons.github.com/myorg1.png" },
                new AccountDesigner { Login = "MyOrg2", AvatarUrl = "https://identicons.github.com/myorg2.png"  },
                new AccountDesigner { Login = "MyOrg3", AvatarUrl = "https://identicons.github.com/myorg3.png"  },
                new AccountDesigner { Login = "a-long-org-name", AvatarUrl = "https://identicons.github.com/a-long-org-name.png"  },
            };

            ExistingForks = new[]
            {
                new RemoteRepositoryModelDesigner { Owner = "MyOrg5", Name = "MyRepo" },
                new RemoteRepositoryModelDesigner { Owner = "MyOrg6", Name = "MyRepo" },
            };
        }

        public IReadOnlyList<IAccount> Accounts { get; set; }

        public IObservable<object> Done => null;

        public IReadOnlyList<RemoteRepositoryModel> ExistingForks { get; set; }

        public bool IsLoading { get; set; }

        public string Title => null;

        public ReactiveCommand<IAccount, IAccount> SelectedAccount => null;

        public ReactiveCommand<RemoteRepositoryModel, Unit> SwitchOrigin => null;

        public Task InitializeAsync(LocalRepositoryModel repository, IConnection connection)
        {
            return Task.CompletedTask;
        }
    }
}