using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.Validation;
using GitHub.ViewModels;
using GitHub.VisualStudio.TeamExplorer.Home;
using ReactiveUI;
using GitHub.VisualStudio.TeamExplorer.Connect;
using System.Collections.ObjectModel;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class BaseViewModelDesigner : ReactiveObject, IViewModel
    {
        public ICommand Cancel { get; set; }
        public bool IsShowing { get; set; }
        public string Title { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class RepositoryCreationViewModelDesigner : BaseViewModelDesigner, IRepositoryCreationViewModel
    {
        public RepositoryCreationViewModelDesigner()
        {
            RepositoryName = "Hello-World";
            Description = "A description";
            KeepPrivate = true;
            Accounts = new ReactiveList<IAccount>
            {
                new AccountDesigner { Login = "shana" },
                new AccountDesigner { Login = "GitHub", IsUser = false }
            };
            GitIgnoreTemplates = new ReactiveList<GitIgnoreItem>
            {
                GitIgnoreItem.Create("VisualStudio"),
                GitIgnoreItem.Create("Wap"),
                GitIgnoreItem.Create("WordPress")
            };

            Licenses = new ReactiveList<LicenseItem>
            {
                new LicenseItem("agpl-3.0", "GNU Affero GPL v3.0"),
                new LicenseItem("apache-2.0", "Apache License 2.0"),
                new LicenseItem("artistic-2.0", "Artistic License 2.0"),
                new LicenseItem("mit", "MIT License")
            };

            SelectedLicense = LicenseItem.None;
            SelectedGitIgnoreTemplate = null;
        }

        public new string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

        public IReadOnlyList<IAccount> Accounts
        {
            get;
            private set;
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

        public ICommand BrowseForDirectory
        {
            get;
            private set;
        }

        public bool CanKeepPrivate
        {
            get;
            private set;
        }

        public IReactiveCommand<Unit> CreateRepository
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
    }

    [ExcludeFromCodeCoverage]
    public sealed class RepositoryPublishViewModelDesigner : RepositoryCreationViewModelDesigner, IRepositoryPublishViewModel
    {
        class Conn : IConnection
        {
            public HostAddress HostAddress { get; set; }

            public string Username { get; set; }
            public ObservableCollection<ISimpleRepositoryModel> Repositories { get; set;  }

            public IObservable<IConnection> Login()
            {
                return null;
            }

            public void Logout()
            {
            }

            public void Dispose()
            {
            }
        }

        public RepositoryPublishViewModelDesigner()
        {
            Connections = new ReactiveList<IConnection>
            {
                new Conn() { HostAddress = new HostAddress() },
                new Conn() { HostAddress = HostAddress.Create("ghe.io") }
            };
            SelectedConnection = Connections[0];
        }

        public string DefaultRepositoryName
        {
            get
            {
                return "whatever";
            }
        }

        public bool IsHostComboBoxVisible
        {
            get
            {
                return true;
            }
        }

        public bool IsPublishing
        {
            get;
            private set;
        }

        public IReactiveCommand<Unit> PublishRepository
        {
            get;
            private set;
        }

        public ReactiveList<IConnection> Connections
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
    public sealed class RepositoryHostDesigner : ReactiveObject, IRepositoryHost
    {
        public RepositoryHostDesigner(string title)
        {
            this.Title = title;
        }

        public HostAddress Address
        {
            get;
            private set;
        }

        public IApiClient ApiClient
        {
            get;
            private set;
        }

        public bool IsLoggedIn
        {
            get;
            private set;
        }

        public IModelService ModelService
        {
            get;
            private set;
        }

        public string Title
        {
            get;
            private set;
        }

        public void Dispose()
        {
        }

        public IObservable<AuthenticationResult> LogIn(string usernameOrEmail, string password)
        {
            throw new NotImplementedException();
        }

        public IObservable<AuthenticationResult> LogInFromCache()
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> LogOut()
        {
            throw new NotImplementedException();
        }
    }

    [ExcludeFromCodeCoverage]
    public sealed class AccountDesigner : IAccount
    {
        public AccountDesigner()
        {
            Login = "octocat";
            IsUser = true;
        }

        public BitmapSource Avatar
        {
            get
            {
                return IsUser
                    ? AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png")
                    : AvatarProvider.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");
            }
        }

        public bool HasMaximumPrivateRepositories
        {
            get;
            set;
        }

        public bool IsEnterprise
        {
            get;
            set;
        }

        public bool IsOnFreePlan
        {
            get;
            set;
        }

        public bool IsUser
        {
            get;
            set;
        }

        public string Login
        {
            get;
            set;
        }

        public int OwnedPrivateRepos
        {
            get;
            set;
        }

        public long PrivateReposInPlan
        {
            get;
            set;
        }
    }

    [ExcludeFromCodeCoverage]
    public class RepositoryModelDesigner : NotificationAwareObject, IRepositoryModel
    {
        public RepositoryModelDesigner() : this("repo")
        {
        }

        public RepositoryModelDesigner(string name) : this("repo", "github")
        {
            Name = name;
        }

        public RepositoryModelDesigner(string name, string owner)
        {
            Name = name;
            Owner = new AccountDesigner { Login = owner };
        }

        public void SetIcon(bool isPrivate, bool isFork)
        {
        }

        public string Name { get; set; }
        public UriString CloneUrl { get; set; }
        public string LocalPath { get; set; }

        public Octicon Icon { get; set; }

        public IAccount Owner { get; set; }

        public void Refresh() { }
    }

    public class RepositoryCloneViewModelDesigner : BaseViewModelDesigner, IRepositoryCloneViewModel
    {
        public RepositoryCloneViewModelDesigner()
        {
            var repositories = new ReactiveList<IRepositoryModel>
            {
                new RepositoryModelDesigner("encourage", "haacked"),
                new RepositoryModelDesigner("haacked.com", "haacked"),
                new RepositoryModelDesigner("octokit.net", "octokit"),
                new RepositoryModelDesigner("octokit.rb", "octokit"),
                new RepositoryModelDesigner("octokit.objc", "octokit"),
                new RepositoryModelDesigner("windows", "github"),
                new RepositoryModelDesigner("mac", "github"),
                new RepositoryModelDesigner("github", "github")
            };

            BrowseForDirectory = ReactiveCommand.Create();

            FilteredRepositories = repositories.CreateDerivedCollection(
                x => x
            );

            BaseRepositoryPathValidator = this.CreateBaseRepositoryPathValidator();
        }

        public IReactiveCommand<Unit> CloneCommand
        {
            get;
            private set;
        }

        public IRepositoryModel SelectedRepository { get; set; }

        public IReactiveDerivedList<IRepositoryModel> FilteredRepositories
        {
            get;
            private set;
        }

        public bool FilterTextIsEnabled
        {
            get;
            private set;
        }

        public string FilterText { get; set; }

        public new string Title { get { return "Clone a GitHub Repository"; } }

        public bool IsLoading
        {
            get { return false; }
        }

        public IReactiveCommand<IReadOnlyList<IRepositoryModel>> LoadRepositoriesCommand
        {
            get;
            private set;
        }

        public bool LoadingFailed
        {
            get { return false; }
        }

        public bool NoRepositoriesFound
        {
            get;
            set;
        }

        public ICommand BrowseForDirectory
        {
            get;
            private set;
        }

        public string BaseRepositoryPath
        {
            get;
            set;
        }

        public bool CanClone
        {
            get;
            private set;
        }

        public ReactivePropertyValidator<string> BaseRepositoryPathValidator
        {
            get;
            private set;
        }
    }

    public class GitHubHomeSectionDesigner : IGitHubHomeSection
    {
        public GitHubHomeSectionDesigner()
        {
            Icon = Octicon.@lock;
            RepoName = "octokit";
            RepoUrl = "https://github.com/octokit/octokit.net";
        }

        public Octicon Icon
        {
            get;
            private set;
        }

        public string RepoName
        {
            get;
            set;
        }

        public string RepoUrl
        {
            get;
            set;
        }
    }

    public class GitHubConnectSectionDesigner : IGitHubConnectSection
    {
        public GitHubConnectSectionDesigner()
        {
            Repositories = new ObservableCollection<ISimpleRepositoryModel>();
            Repositories.Add(new SimpleRepositoryModel("octokit", new UriString("https://github.com/octokit/octokit.net"), @"C:\Users\user\Source\Repos\octokit.net"));
            Repositories.Add(new SimpleRepositoryModel("cefsharp", new UriString("https://github.com/cefsharp/cefsharp"), @"C:\Users\user\Source\Repos\cefsharp"));
            Repositories.Add(new SimpleRepositoryModel("git-lfs", new UriString("https://github.com/github/git-lfs"), @"C:\Users\user\Source\Repos\git-lfs"));
            Repositories.Add(new SimpleRepositoryModel("another octokit", new UriString("https://github.com/octokit/octokit.net"), @"C:\Users\user\Source\Repos\another-octokit.net"));
            Repositories.Add(new SimpleRepositoryModel("some cefsharp", new UriString("https://github.com/cefsharp/cefsharp"), @"C:\Users\user\Source\Repos\something-else"));
            Repositories.Add(new SimpleRepositoryModel("even more git-lfs", new UriString("https://github.com/github/git-lfs"), @"C:\Users\user\Source\Repos\A different path"));
        }

        public ObservableCollection<ISimpleRepositoryModel> Repositories
        {
            get; set;
        }

        public void DoClone()
        {
        }

        public void DoCreate()
        {
        }

        public void SignOut()
        {
        }

        public void Login()
        {
        }

        public bool OpenRepository()
        {
            return true;
        }

        public IConnection SectionConnection { get; }
    }
}
