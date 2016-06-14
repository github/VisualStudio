
using System;
using GitHub.Caches;
using Octokit;
using Xunit;

public class AccountCacheItemTests
{
    public class TheConstructor : TestBaseClass
    {
        [Theory]
        [InlineData("https://foo.com", true)]
        [InlineData("https://notgithub.com", true)]
        [InlineData("https://github.com", false)]
        [InlineData("https://api.github.com", false)]
        [InlineData("GARBAGE", false)]
        public void SetsIsEnterpriseCorrectly(string htmlUrl, bool expected)
        {
            var apiAccount = CreateOctokitUser(htmlUrl);
            var cachedAccount = new AccountCacheItem(apiAccount);

            Assert.Equal(expected, cachedAccount.IsEnterprise);
        }
    }

    static User CreateOctokitUser(string url)
    {
        return new User("https://url", "", "", 1, "GitHub", DateTimeOffset.UtcNow, 0, "email", 100, 100, true, url, 10, 42, "somewhere", "foo", "Who cares", 1, new Plan(), 1, 1, 1, "https://url", false, null, null);
    }
}
