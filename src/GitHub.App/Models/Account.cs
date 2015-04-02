using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
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

        public Account(IRepositoryHost host, User user)
            : this(host)
        {
            Id = user.Id;
            IsUser = true;

            Update(user);
        }

        public Account(IRepositoryHost host, Organization organization)
            : this(host)
        {
            Id = organization.Id;
            IsUser = false;

            Update(organization);
        }

        private Account(IRepositoryHost host)
        {
            Host = host;

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

        public bool IsEnterprise { get { return Host.IsEnterprise; } }

        public bool IsGitHub { get { return Host.IsGitHub; } }

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

        public void Update(User user)
        {
            UpdateAccountInfo(user);
        }

        public void Update(Organization organization)
        {
            UpdateAccountInfo(organization);
        }

        void UpdateAccountInfo(Octokit.Account githubAccount)
        {
            if (Id != githubAccount.Id) return;

            Email = githubAccount.Email;
            Login = githubAccount.Login;
            Name = githubAccount.Name ?? githubAccount.Login;
            OwnedPrivateRepos = githubAccount.OwnedPrivateRepos;
            PrivateReposInPlan = (githubAccount.Plan == null ? 0 : githubAccount.Plan.PrivateRepos);
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
