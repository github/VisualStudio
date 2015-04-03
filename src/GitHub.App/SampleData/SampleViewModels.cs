using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
using Account = Octokit.Account;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class RepositoryCreationViewModelDesigner : ReactiveObject, IRepositoryCreationViewModel
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

        public string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

        public ReactiveList<IAccount> Accounts
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

        public ICommand Cancel { get; set; }
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

        public ReactiveList<IAccount> Accounts
        {
            get;
            private set;
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

        public IHostCache Cache
        {
            get;
            private set;
        }

        public bool IsEnterprise
        {
            get;
            private set;
        }

        public bool IsGitHub
        {
            get;
            private set;
        }

        public bool IsLocal
        {
            get;
            private set;
        }

        public bool IsLoggedIn
        {
            get;
            private set;
        }

        public bool IsLoggingIn
        {
            get;
            private set;
        }

        public ReactiveList<IAccount> Organizations
        {
            get;
            private set;
        }

        public string Title
        {
            get;
            private set;
        }

        public IAccount User
        {
            get;
            private set;
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

        public IObservable<Unit> Refresh()
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> Refresh(Func<IRepositoryHost, IObservable<Unit>> refreshTrackedRepositoriesFunc)
        {
            throw new NotImplementedException();
        }
    }

    [ExcludeFromCodeCoverage]
    public sealed class AccountDesigner : ReactiveObject, IAccount
    {
        public AccountDesigner()
        {
        }

        public AccountDesigner(string name)
        {
            Name = name;
            Avatar = new AvatarProviderDesigner().DefaultOrgBitmapImage;
            IsGitHubStaff = false;
            IsSiteAdmin = false;
        }

        public object Avatar { get; set; }
        public string Email { get; set; }
        public int Id { get; set; }
        public bool IsEnterprise { get; set; }
        public bool IsGitHub { get; set; }
        public bool IsLocal { get; set; }
        public bool IsOnFreePlan { get; set; }
        public bool HasMaximumPrivateRepositories { get; private set; }
        public bool IsSelected { get; set; }
        public bool IsUser { get; set; }
        public bool IsSiteAdmin { get; private set; }
        public bool IsGitHubStaff { get; private set; }
        public IRepositoryHost Host { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public int OwnedPrivateRepos { get; set; }
        public long PrivateReposInPlan { get; set; }

        public void Update(User ghUser)
        {
            throw new NotImplementedException();
        }

        public void Update(Organization org)
        {
            throw new NotImplementedException();
        }
    }


    [ExcludeFromCodeCoverage]
    public class AvatarProviderDesigner : IAvatarProvider
    {
        public AvatarProviderDesigner()
        {
            DefaultUserBitmapImage = ImageHelper.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png");
            DefaultOrgBitmapImage = ImageHelper.CreateBitmapImage("pack://application:,,,/GitHub.App;component/Images/default_org_avatar.png");
        }

        public BitmapImage DefaultUserBitmapImage { get; private set; }
        public BitmapImage DefaultOrgBitmapImage { get; private set; }

        public IObservable<BitmapSource> GetAvatar(Account apiAccount)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> InvalidateAvatar(Account apiAccount)
        {
            throw new NotImplementedException();
        }

        public IObservable<BitmapSource> GetAvatar(string email)
        {
            throw new NotImplementedException();
        }
    }

    [ExcludeFromCodeCoverage]
    public class RepositoryModelDesigner : ReactiveObject, IRepositoryModel
    {
        public RepositoryModelDesigner()
        {
        }

        public RepositoryModelDesigner(string name)
        {
            Name = name;
        }

        public RepositoryModelDesigner(string name, string owner)
        {
            Owner = owner;
            Name = name;
            OwnerWithSlash = owner + "/";
            NameWithOwner = OwnerWithSlash + name;
            HasRemote = IsHosted = true;
            AdditionalClones = new HashSet<string>();
            ToolTip = "Repo Tooltip";
            IsPrivate = true;
            CanViewOnHost = true;
        }

        public DateTimeOffset? LastShadowBackupTime { get; set; }
        public string LastShadowBackupSha1 { get; set; }
        public bool IsSelected { get; set; }
        public HostAddress HostAddress { get; private set; }
        public int? Id { get; set; }
        public bool IsLostOnDisk { get; set; }
        public string LocalWorkingDirectory { get; set; }
        public string LocalDotGitPath { get; set; }
        public UriString CloneUrl { get; set; }
        public UriString HttpsUrl { get; set; }
        public UriString SshUrl { get; set; }
        public UriString UpstreamCloneUrl { get; set; }
        public bool HasRemote { get; set; }
        public bool IsHosted { get; set; }
        public bool HasLocal { get; set; }
        public bool CanViewOnHost { get; private set; }
        public IRepositoryHost RepositoryHost { get; set; }

        string IRepositoryModel.Owner
        {
            get { return Owner; }
            set { Owner = value; }
        }

        public int? OwnerId { get; set; }

        public string Owner { get; set; }
        public string OwnerWithSlash { get; set; }
        public string Name { get; set; }
        public string NameWithOwner { get; set; }
        public string Description { get; set; }
        public string ToolTip { get; set; }
        public Uri HostUri { get; set; }
        public bool IsCollaborator { get; set; }
        public bool IsCloning { get; set; }
        public bool IsFork { get; private set; }
        public bool HasDeployedGitIgnore { get; set; }
        public string NonGitHubRemoteHost { get; set; }
        public HashSet<string> AdditionalClones { get; private set; }

        public bool IsPrivate { get; set; }

        public string Group { get; set; }

        public bool HasLocalClone { get; private set; }
    }

    public class RepositoryCloneViewModelDesigner : IRepositoryCloneViewModel
    {
        public RepositoryCloneViewModelDesigner()
        {
            var repositories = new ReactiveList<IRepositoryModel>
            {
                new RepositoryModel {Owner = "haacked", Name = "encourage" },
                new RepositoryModel {Owner = "haacked", Name = "haacked.com" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.net" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.rb" },
                new RepositoryModel {Owner = "octokit", Name = "octokit.objc" },
                new RepositoryModel {Owner = "github", Name = "windows" },
                new RepositoryModel {Owner = "github", Name = "mac" },
                new RepositoryModel {Owner = "github", Name = "github" }
            };

            FilteredRepositories = repositories.CreateDerivedCollection(
                x => x
            );
        }

        public ICommand Cancel
        {
            get;
            private set;
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

        public string Title { get { return "Clone a GitHub Repository"; } }
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
