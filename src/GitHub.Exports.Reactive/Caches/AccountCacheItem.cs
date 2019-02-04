using System;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;
using Account = Octokit.Account;

namespace GitHub.Caches
{
    public class AccountCacheItem : IAvatarContainer
    {
        public static AccountCacheItem Create(Account apiAccount)
        {
            return new AccountCacheItem(apiAccount);
        }

        public AccountCacheItem()
        { }

        public AccountCacheItem(Account account)
        {
            Login = account.Login;
            IsUser = (account as User) != null;
            Uri htmlUrl;
            IsEnterprise = Uri.TryCreate(account.HtmlUrl, UriKind.Absolute, out htmlUrl)
                && !HostAddress.IsGitHubDotComUri(htmlUrl);
            PrivateRepositoriesInPlanCount = account.Plan != null ? account.Plan.PrivateRepos : 0;
            OwnedPrivateRepositoriesCount = account.OwnedPrivateRepos;
            AvatarUrl = account.AvatarUrl;
        }

        public string Login { get; set; }
        public bool IsUser { get; set; }
        public bool IsEnterprise { get; set; }
        public int OwnedPrivateRepositoriesCount { get; set; }
        public long PrivateRepositoriesInPlanCount { get; set; }
        public string AvatarUrl { get; set; }
    }
}
