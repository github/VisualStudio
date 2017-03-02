using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Windows.Input;
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
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class RepositoryCreationViewModelDesigner : DialogViewModelBase, IRepositoryCreationViewModel
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

        public new string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

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
            public ObservableCollection<ILocalRepositoryModel> Repositories { get; set;  }

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
            Connections = new ObservableCollection<IConnection>
            {
                new Conn() { HostAddress = new HostAddress() },
                new Conn() { HostAddress = HostAddress.Create("ghe.io") }
            };
            SelectedConnection = Connections[0];
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

        public IReactiveCommand<ProgressState> PublishRepository
        {
            get;
            private set;
        }

        public ObservableCollection<IConnection> Connections
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

        public bool SupportsGist
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
    public static class RepositoryModelDesigner
    {
        public static IRemoteRepositoryModel Create(string name = null, string owner = null)
        {
            name = name ?? "octocat";
            owner = owner ?? "github";
            return new RemoteRepositoryModel(0, name, new UriString("http://github.com/" + name + "/" + owner), false, false, new AccountDesigner() { Login = owner });
        }
    }

    public class RepositoryCloneViewModelDesigner : DialogViewModelBase, IRepositoryCloneViewModel
    {
        public RepositoryCloneViewModelDesigner()
        {
            Repositories = new ObservableCollection<IRemoteRepositoryModel>
            {
                RepositoryModelDesigner.Create("encourage", "haacked"),
                RepositoryModelDesigner.Create("haacked.com", "haacked"),
                RepositoryModelDesigner.Create("octokit.net", "octokit"),
                RepositoryModelDesigner.Create("octokit.rb", "octokit"),
                RepositoryModelDesigner.Create("octokit.objc", "octokit"),
                RepositoryModelDesigner.Create("windows", "github"),
                RepositoryModelDesigner.Create("mac", "github"),
                RepositoryModelDesigner.Create("github", "github")
            };

            BrowseForDirectory = ReactiveCommand.Create();

            BaseRepositoryPathValidator = ReactivePropertyValidator.ForObservable(this.WhenAny(x => x.BaseRepositoryPath, x => x.Value))
                .IfNullOrEmpty("Please enter a repository path")
                .IfTrue(x => x.Length > 200, "Path too long")
                .IfContainsInvalidPathChars("Path contains invalid characters")
                .IfPathNotRooted("Please enter a valid path");
        }

        public IReactiveCommand<object> CloneCommand
        {
            get;
            private set;
        }

        public IRepositoryModel SelectedRepository { get; set; }

        public ObservableCollection<IRemoteRepositoryModel> Repositories
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

        public IReactiveCommand<IReadOnlyList<IRemoteRepositoryModel>> LoadRepositoriesCommand
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
            Icon = Octicon.repo;
            RepoName = "octokit";
            RepoUrl = "https://github.com/octokit/something-really-long-here-to-check-for-trimming";
            IsLoggedIn = false;
        }

        public Octicon Icon
        {
            get;
            private set;
        }

        public bool IsLoggedIn
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

        public void Login()
        {

        }

        public ICommand OpenOnGitHub { get; }
    }

    public class GitHubConnectSectionDesigner : IGitHubConnectSection
    {
        public GitHubConnectSectionDesigner()
        {
            Repositories = new ObservableCollection<ILocalRepositoryModel>();
            Repositories.Add(new LocalRepositoryModel("octokit", new UriString("https://github.com/octokit/octokit.net"), @"C:\Users\user\Source\Repos\octokit.net"));
            Repositories.Add(new LocalRepositoryModel("cefsharp", new UriString("https://github.com/cefsharp/cefsharp"), @"C:\Users\user\Source\Repos\cefsharp"));
            Repositories.Add(new LocalRepositoryModel("git-lfs", new UriString("https://github.com/github/git-lfs"), @"C:\Users\user\Source\Repos\git-lfs"));
            Repositories.Add(new LocalRepositoryModel("another octokit", new UriString("https://github.com/octokit/octokit.net"), @"C:\Users\user\Source\Repos\another-octokit.net"));
            Repositories.Add(new LocalRepositoryModel("some cefsharp", new UriString("https://github.com/cefsharp/cefsharp"), @"C:\Users\user\Source\Repos\something-else"));
            Repositories.Add(new LocalRepositoryModel("even more git-lfs", new UriString("https://github.com/github/git-lfs"), @"C:\Users\user\Source\Repos\A different path"));
        }

        public ObservableCollection<ILocalRepositoryModel> Repositories
        {
            get; set;
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
        public ICommand Clone { get; }
    }

    public class InfoPanelDesigner
    {
        public string Message => "This is an informational message for the [info panel](link) to test things in design mode.";
        public MessageType MessageType => MessageType.Information;
    }
}
