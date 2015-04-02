using System;
using System.Diagnostics;
using System.Globalization;
using Octokit;
using ReactiveUI;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Account : ReactiveObject, IAccount
    {
        string email;
        readonly ObservableAsPropertyHelper<bool> isOnFreePlan;
        readonly ObservableAsPropertyHelper<bool> hasMaximumPrivateRepositoriesLeft;
        string login;
        string name;
        int ownedPrivateRepos;
        long privateReposInPlan;
        
        public Account(Octokit.Account apiAccount, bool isGitHub)
        {
            Id = apiAccount.Id;
            IsGitHub = isGitHub;
            IsUser = (apiAccount as User) == null;
            Email = apiAccount.Email;
            Login = apiAccount.Login;
            Name = apiAccount.Name ?? apiAccount.Login;
            OwnedPrivateRepos = apiAccount.OwnedPrivateRepos;
            PrivateReposInPlan = (apiAccount.Plan == null ? 0 : apiAccount.Plan.PrivateRepos);

            isOnFreePlan = this.WhenAny(x => x.PrivateReposInPlan, x => x.Value == 0)
                .ToProperty(this, x => x.IsOnFreePlan);

            hasMaximumPrivateRepositoriesLeft = this.WhenAny(
                x => x.OwnedPrivateRepos,
                x => x.PrivateReposInPlan,
                (owned, avalible) => owned.Value >= avalible.Value)
                .ToProperty(this, x => x.HasMaximumPrivateRepositories);
        }

        public string Email
        {
            get { return email; }
            private set { this.RaiseAndSetIfChanged(ref email, value); }
        }

        public IRepositoryHost Host { get; private set; }

        public int Id { get; private set; }

        public bool IsEnterprise { get { return !IsGitHub; } }

        public bool IsGitHub { get; private set; }

        public bool IsOnFreePlan
        {
            get { return isOnFreePlan.Value; }
        }

        public bool HasMaximumPrivateRepositories
        {
            get { return hasMaximumPrivateRepositoriesLeft.Value; }
        }

        public bool IsUser { get; private set; }

        public string Login
        {
            get { return login; }
            private set { this.RaiseAndSetIfChanged(ref login, value); }
        }

        public string Name
        {
            get { return name; }
            private set { this.RaiseAndSetIfChanged(ref name, value); }
        }

        public int OwnedPrivateRepos
        {
            get { return ownedPrivateRepos; }
            private set { this.RaiseAndSetIfChanged(ref ownedPrivateRepos, value); }
        }

        public long PrivateReposInPlan
        {
            get { return privateReposInPlan; }
            private set { this.RaiseAndSetIfChanged(ref privateReposInPlan, value); }
        }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,
                    "Account: Login: {0} Name: {1}, Id: {2} Id: ", Login, Name, Id);
            }
        }

    }
}
