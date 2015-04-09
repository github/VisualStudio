using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.Validation;
using GitHub.ViewModels;
using GitHub.VisualStudio.TeamExplorerHome;
using Octokit;
using ReactiveUI;

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
            Accounts = new ReactiveList<IAccount> { new AccountDesigner("GitHub") };
            GitIgnoreTemplates = new ReactiveList<GitIgnoreItem>
            {
                GitIgnoreItem.Create("VisualStudio"),
                GitIgnoreItem.Create("Wap"),
                GitIgnoreItem.Create("WordPress")
            };

            Licenses = new ReactiveList<LicenseItem>
            {
                new LicenseItem(new LicenseMetadata("agpl-3.0", "GNU Affero GPL v3.0", new Uri("https://whatever"))),
                new LicenseItem(new LicenseMetadata("apache-2.0", "Apache License 2.0", new Uri("https://whatever"))),
                new LicenseItem(new LicenseMetadata("artistic-2.0", "Artistic License 2.0", new Uri("https://whatever"))),
                new LicenseItem(new LicenseMetadata("mit", "MIT License", new Uri("https://whatever")))
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

        public ReactiveList<GitIgnoreItem> GitIgnoreTemplates
        {
            get; private set;
        }

        public ReactiveList<LicenseItem> Licenses
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
        public RepositoryPublishViewModelDesigner()
        {
            var gitHubHost = new RepositoryHostDesigner("GitHub");
            RepositoryHosts = new ReactiveList<IRepositoryHost>
            {
                gitHubHost,
                new RepositoryHostDesigner("ghe.io")
            };
            SelectedHost = gitHubHost;
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

        public ReactiveList<IRepositoryHost> RepositoryHosts
        {
            get;
            private set;
        }

        public IRepositoryHost SelectedHost
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

        public string Title
        {
            get;
            private set;
        }

        public IObservable<IReadOnlyList<IAccount>> GetAccounts(IAvatarProvider avatarProvider)
        {
            throw new NotImplementedException();
        }

        public IObservable<IEnumerable<CachedAccount>> GetAllOrganizations()
        {
            throw new NotImplementedException();
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
    public sealed class AccountDesigner : GitHub.Models.Account
    {
        public AccountDesigner() : this("some-name")
        {
        }

        public AccountDesigner(string login)
            : base(new CachedAccount { Login = login },
                  Observable.Return(ImageHelper.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png")))
        {
        }
    }

    [ExcludeFromCodeCoverage]
    public class RepositoryModelDesigner : IRepositoryModel
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
            Owner = new AccountDesigner(owner);
        }

        public UriString CloneUrl { get; set; }

        public Octicon Icon { get; set; }

        public string Name { get; set; }

        public IAccount Owner { get; set; }
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

            FilteredRepositories = repositories.CreateDerivedCollection(
                x => x
            );
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
}
