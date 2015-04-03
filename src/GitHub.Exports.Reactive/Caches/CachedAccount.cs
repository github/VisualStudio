using System;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Caches
{
    /// <summary>
    /// Used to cache and restore account information.
    /// </summary>
    public class CachedAccount
    {
        public CachedAccount()
        {
        }

        public CachedAccount(Account account)
        {
            Id = account.Id;
            Login = account.Login;
            OwnedPrivateRepos = account.OwnedPrivateRepos;
            PrivateReposInPlan = (account.Plan == null ? 0 : account.Plan.PrivateRepos);
            IsUser = (account as User) != null;
            PrivateReposInPlan = account.Plan != null ? account.Plan.PrivateRepos : 0;
            OwnedPrivateRepos = account.OwnedPrivateRepos;

            Uri url;
            IsEnterprise = (Uri.TryCreate(account.HtmlUrl, UriKind.Absolute, out url))
                && HostAddress.Create(url) != HostAddress.GitHubDotComHostAddress;
        }

        public int Id { get; set; }
        public string Login { get; set; }
        public bool IsUser { get; set; }
        public bool IsEnterprise { get; set; }
        public int OwnedPrivateRepos { get; set; }
        public long PrivateReposInPlan { get; set; }
    }
}
