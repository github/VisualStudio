using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Windows.Media.Imaging;
using GitHub.Helpers;
using GitHub.Models;
using Octokit;
using ReactiveUI;
using Account = Octokit.Account;
using GitHub.Services;
using System.Collections.Generic;
using GitHub.Primitives;
using NullGuard;
using GitHub.UI;
using System.Windows.Input;
using GitHub.Validation;

namespace GitHub.SampleData
{

    [ExcludeFromCodeCoverage]
    public class CreateRepoViewModelDesigner : ReactiveObject, ICreateRepoViewModel
    {
        public CreateRepoViewModelDesigner()
        {
            RepositoryName = "Hello-World";
            Description = "A description";
            KeepPrivate = true;
            Accounts = new ReactiveList<IAccount> { new AccountDesigner("GitHub") };
        }

        public string RepositoryName { get; private set; }
        public string SafeRepositoryName { get; private set; }
        public bool ShowRepositoryNameWarning { get; private set; }
        public string RepositoryNameWarningText { get; private set; }
        public ReactivePropertyValidator<string> RepositoryNameValidator { get; private set; }
        public string Description { get; set; }
        public ReactivePropertyValidator<IAccount> SelectedAccountValidator { get; private set; }
        public bool KeepPrivate { get; set; }
        public bool CanKeepPrivate { get; private set; }
        public bool ShowUpgradeToMicroPlanWarning { get; private set; }
        public bool ShowUpgradePlanWarning { get; private set; }
        public ReactiveCommand<Unit> CreateRepository { get; private set; }
        public bool IsPublishing { get; private set; }
        public ReactiveCommand<Object> UpgradeAccountPlan { get; private set; }
        public ReactiveCommand<Object> Reset { get; private set; }
        public ReactiveList<IAccount> Accounts { get; private set; }
        public IAccount SelectedAccount { get; private set; }

        public ICommand OkCmd { get; private set; }
        public ICommand CancelCmd { get; private set; }
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
    }
}
