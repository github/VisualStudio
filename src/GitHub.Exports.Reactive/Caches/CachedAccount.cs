using System;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Caches
{
    /// <summary>
    /// Used to cache and restore account information.
    /// </summary>
    public class CachedAccount : IAvatarContainer
    {
        public CachedAccount()
        {
        }

        public CachedAccount(Account account)
        {
            Login = account.Login;
            OwnedPrivateRepos = account.OwnedPrivateRepos;
            PrivateReposInPlan = (account.Plan == null ? 0 : account.Plan.PrivateRepos);
            IsUser = (account as User) != null;
            PrivateReposInPlan = account.Plan != null ? account.Plan.PrivateRepos : 0;
            OwnedPrivateRepos = account.OwnedPrivateRepos;
            Uri avatarUrl;
            AvatarUrl = Uri.TryCreate(account.AvatarUrl, UriKind.Absolute, out avatarUrl)
                ? avatarUrl
                : null;

            Uri url;
            IsEnterprise = (Uri.TryCreate(account.HtmlUrl, UriKind.Absolute, out url))
                && HostAddress.Create(url) != HostAddress.GitHubDotComHostAddress;
        }

        public string Login { get; set; }
        public bool IsUser { get; set; }
        public bool IsEnterprise { get; set; }
        public int OwnedPrivateRepos { get; set; }
        public long PrivateReposInPlan { get; set; }
        public Uri AvatarUrl { get; set; }
    }
}
