
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
            var apiAccount = CreateOctokitUser("foo", htmlUrl);
            var cachedAccount = new AccountCacheItem(apiAccount);

            Assert.Equal(expected, cachedAccount.IsEnterprise);
        }
    }
}
