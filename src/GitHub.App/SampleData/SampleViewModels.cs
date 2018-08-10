using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public IReactiveCommand<CreateRepositoryDialogResult> CreateRepository
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
            public ObservableCollection<ILocalRepositoryModel> Repositories { get; set; }

            public Octokit.User User => null;
            public bool IsLoggedIn => true;

            public Exception ConnectionError => null;
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

        public IReactiveCommand<ProgressState> PublishRepository
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
        public static IRemoteRepositoryModel Create(string name = null, string owner = null)
        {
            name = name ?? "octocat";
            owner = owner ?? "github";
            return new RemoteRepositoryModel(0, name, new UriString("http://github.com/" + name + "/" + owner), false, false, new AccountDesigner() { Login = owner }, null);
        }
    }

    public class RepositoryCloneViewModelDesigner : ViewModelBase, IRepositoryCloneViewModel
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

        public string Title { get { return "Clone a GitHub Repository"; } }

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

        public IObservable<object> Done { get; }

        public Task InitializeAsync(IConnection connection) => Task.CompletedTask;
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
            Repositories.Add(new LocalRepositoryModel("octokit", new UriString("https://github.com/octokit/octokit.net"), @"C:\Users\user\Source\Repos\octokit.net", new GitServiceDesigner()));
            Repositories.Add(new LocalRepositoryModel("cefsharp", new UriString("https://github.com/cefsharp/cefsharp"), @"C:\Users\user\Source\Repos\cefsharp", new GitServiceDesigner()));
            Repositories.Add(new LocalRepositoryModel("git-lfs", new UriString("https://github.com/github/git-lfs"), @"C:\Users\user\Source\Repos\git-lfs", new GitServiceDesigner()));
            Repositories.Add(new LocalRepositoryModel("another octokit", new UriString("https://github.com/octokit/octokit.net"), @"C:\Users\user\Source\Repos\another-octokit.net", new GitServiceDesigner()));
            Repositories.Add(new LocalRepositoryModel("some cefsharp", new UriString("https://github.com/cefsharp/cefsharp"), @"C:\Users\user\Source\Repos\something-else", new GitServiceDesigner()));
            Repositories.Add(new LocalRepositoryModel("even more git-lfs", new UriString("https://github.com/github/git-lfs"), @"C:\Users\user\Source\Repos\A different path", new GitServiceDesigner()));
        }

        public ObservableCollection<ILocalRepositoryModel> Repositories
        {
            get; set;
        }

        public Task DoCreate()
        {
            return Task.CompletedTask;
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
