using System;
using System.Diagnostics;
using System.Globalization;
using GitHub.Caches;
using ReactiveUI;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Account : ReactiveObject, IAccount
    {
        public Account(CachedAccount cachedAccount)
        {
            IsUser = cachedAccount.IsUser;
            Login = cachedAccount.Login;
            OwnedPrivateRepos = cachedAccount.OwnedPrivateRepos;
            PrivateReposInPlan = cachedAccount.PrivateReposInPlan;
            IsOnFreePlan = cachedAccount.PrivateReposInPlan == 0;
            HasMaximumPrivateRepositories = OwnedPrivateRepos >= PrivateReposInPlan;
        }

        public bool IsOnFreePlan { get; private set; }

        public bool HasMaximumPrivateRepositories { get; private set; }

        public bool IsUser { get; private set; }

        public bool IsEnterprise { get; private set; }

        public string Login { get; private set; }

        public int OwnedPrivateRepos { get; private set; }

        public long PrivateReposInPlan { get; private set; }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,
                    "Account: Login: {0} IsUser: {1}", Login, IsUser);
            }
        }

    }
}
