
using System;
using GitHub.Caches;
using Octokit;
using NUnit.Framework;

public class AccountCacheItemTests
{
    public class TheConstructor : TestBaseClass
    {
        [TestCase("https://foo.com", true)]
        [TestCase("https://notgithub.com", true)]
        [TestCase("https://github.com", false)]
        [TestCase("https://api.github.com", false)]
        [TestCase("GARBAGE", false)]
        public void SetsIsEnterpriseCorrectly(string htmlUrl, bool expected)
        {
            var apiAccount = CreateOctokitUser("foo", htmlUrl);
            var cachedAccount = new AccountCacheItem(apiAccount);

            Assert.That(expected, Is.EqualTo(cachedAccount.IsEnterprise));
        }
    }
}
