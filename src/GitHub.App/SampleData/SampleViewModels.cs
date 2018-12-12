using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.UI;
using GitHub.Validation;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using GitHub.ViewModels.Dialog.Clone;
using GitHub.ViewModels.TeamExplorer;
using GitHub.VisualStudio.TeamExplorer.Connect;
using GitHub.VisualStudio.TeamExplorer.Home;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class RepositoryCreationViewModelDesigner : ViewModelBase, IRepositoryCreationViewModel
    {
        public RepositoryCreationViewModelDesigner()
        {
            RepositoryName = "Hello-World";
            Description = "A description";
            KeepPrivate = true;
            CanKeepPrivate = true;
            Accounts = new ReactiveList<IAccount>
            {
                new AccountDesigner { Login = "shana" },
                new AccountDesigner { Login = "GitHub", IsUser = false }
            };
            SelectedAccount = Accounts[0];
            GitIgnoreTemplates = new ReactiveList<GitIgnoreItem>
            {
                GitIgnoreItem.Create("VisualStudio"),
                GitIgnoreItem.Create("Wap"),
                GitIgnoreItem.Create("WordPress")
            };
            SelectedGitIgnoreTemplate = GitIgnoreTemplates[0];
            Licenses = new ReactiveList<LicenseItem>
            {
                new LicenseItem("agpl-3.0", "GNU Affero GPL v3.0"),
                new LicenseItem("apache-2.0", "Apache License 2.0"),
                new LicenseItem("artistic-2.0", "Artistic License 2.0"),
                new LicenseItem("mit", "MIT License")
            };

            SelectedLicense = Licenses[0];
        }

        public string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

        public IReadOnlyList<IAccount> Accounts
        {
            get;
            set;
        }

        public string BaseRepositoryPath
        {
            get;
            set;
        }

        public ReactivePropertyValidator<string> BaseRepositoryPathValidator
        {
            get;
            private set;
        }

        public ReactiveCommand<Unit, Unit> BrowseForDirectory
        {
            get;
            private set;
        }

        public bool CanKeepPrivate
        {
            get;
            private set;
        }

        public ReactiveCommand<Unit, Unit> CreateRepository
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            set;
        }

        public bool IsCreating
        {
            get;
            private set;
        }

        public bool KeepPrivate
        {
            get;
            set;
        }

        public string RepositoryName
        {
            get;
            set;
        }

        public ReactivePropertyValidator<string> RepositoryNameValidator
        {
            get;
            private set;
        }

        public ICommand Reset
        {
            get;
            private set;
        }

        public string SafeRepositoryName
        {
            get;
            private set;
        }

        public ReactivePropertyValidator<string> SafeRepositoryNameWarningValidator
        {
            get;
            private set;
        }

        public IAccount SelectedAccount
        {
            get;
            set;
        }

        public bool ShowUpgradePlanWarning
        {
            get;
            private set;
        }

        public bool ShowUpgradeToMicroPlanWarning
        {
            get;
            private set;
        }

        public ICommand UpgradeAccountPlan
        {
            get;
            private set;
        }

        public IReadOnlyList<GitIgnoreItem> GitIgnoreTemplates
        {
            get; private set;
        }

        public IReadOnlyList<LicenseItem> Licenses
        {
            get; private set;
        }

        public GitIgnoreItem SelectedGitIgnoreTemplate
        {
            get;
            set;
        }

        public LicenseItem SelectedLicense
        {
            get;
            set;
        }

        public IObservable<object> Done { get; }

        public Task InitializeAsync(IConnection connection) => Task.CompletedTask;
    }

    [ExcludeFromCodeCoverage]
    public sealed class RepositoryPublishViewModelDesigner : RepositoryCreationViewModelDesigner, IRepositoryPublishViewModel
    {
        class Conn : IConnection
        {
            public HostAddress HostAddress { get; set; }

            public string Username { get; set; }
            public Octokit.User User => null;
            public ScopesCollection Scopes => null;
            public bool IsLoggedIn => true;
            public bool IsLoggingIn => false;

            public Exception ConnectionError => null;

            event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
            {
                add { }
                remove { }
            }
        }

        public RepositoryPublishViewModelDesigner()
        {
            Connections = new ObservableCollectionEx<IConnection>
            {
                new Conn() { HostAddress = new HostAddress() },
                new Conn() { HostAddress = HostAddress.Create("ghe.io") }
            };
            SelectedConnection = Connections[0];
        }

        public bool IsBusy { get; set; }

        public bool IsHostComboBoxVisible
        {
            get
            {
                return true;
            }
        }

        public ReactiveCommand<Unit, ProgressState> PublishRepository
        {
            get;
            private set;
        }

        public IReadOnlyObservableCollection<IConnection> Connections
        {
            get;
            private set;
        }

        public IConnection SelectedConnection
        {
            get; set;
        }
    }

    [ExcludeFromCodeCoverage]
    public static class RepositoryModelDesigner
    {
        public static RemoteRepositoryModel Create(string name = null, string owner = null)
        {
            name = name ?? "octocat";
            owner = owner ?? "github";
            return new RemoteRepositoryModel(0, name, new UriString("http://github.com/" + name + "/" + owner), false, false, new AccountDesigner() { Login = owner }, null);
        }
    }
}
