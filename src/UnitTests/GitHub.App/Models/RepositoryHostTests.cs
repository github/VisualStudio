using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using Octokit;
using UnitTests.Helpers;
using Xunit;

public class RepositoryHostTests
{
    public class TheLoginMethod
    {
        [Fact]
        public async Task SetsTheLoggedInBitWhenLoginIsSuccessful()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            apiClient.GetOrCreateApplicationAuthenticationCode(Arg.Any<Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>>>(), Args.Boolean)
                .Returns(Observable.Return(new ApplicationAuthorization("1234")));
            apiClient.GetUser().Returns(Observable.Return(new User()));
            var hostCache = new InMemoryBlobCache();
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache, Substitute.For<ITwoFactorChallengeHandler>());

            await host.LogIn("aUsername", "aPassowrd");

            Assert.True(host.IsLoggedIn);
        }

        [Fact]
        public async Task CachesTheLoggedInUserWhenLoginIsSuccessful()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            apiClient.GetOrCreateApplicationAuthenticationCode(Arg.Any<Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>>>(), Args.Boolean)
                .Returns(Observable.Return(new ApplicationAuthorization("S3CR3TS")));
            apiClient.GetUser().Returns(Observable.Return(CreateOctokitUser("lagavulin")));
            var hostCache = new InMemoryBlobCache();
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache, Substitute.For<ITwoFactorChallengeHandler>());

            await host.LogIn("aUsername", "aPassword");

            var user = await hostCache.GetObject<CachedAccount>("user");
            Assert.NotNull(user);
            Assert.Equal("lagavulin", user.Login);
        }
    }

    public class TheGetAccountsMethod
    {
        [Fact]
        public async Task ReturnsUsersAndOrganizations()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            apiClient.GetUser().Returns(Observable.Return(CreateOctokitUser("lagavulin")));
            apiClient.GetOrganizations().Returns(new Organization[]
            {
                CreateOctokitOrganization("illuminati"),
                CreateOctokitOrganization("islay"),
                CreateOctokitOrganization("github")
            }.ToObservable());
            var hostCache = new InMemoryBlobCache();
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache, Substitute.For<ITwoFactorChallengeHandler>());

            var accounts = await host.GetAccounts(Substitute.For<IAvatarProvider>());

            Assert.Equal(4, accounts.Count);
            Assert.Equal("lagavulin", accounts[0].Login);
            Assert.Equal("illuminati", accounts[1].Login);
            Assert.Equal("islay", accounts[2].Login);
            Assert.Equal("github", accounts[3].Login);
        }

        [Fact]
        public async Task ReturnsUsersAndOrganizationsFromCacheThenFetch()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            apiClient.GetUser().Returns(Observable.Return(CreateOctokitUser("lagavulin")));
            apiClient.GetOrganizations().Returns(new Organization[]
            {
                CreateOctokitOrganization("illuminati"),
                CreateOctokitOrganization("islay"),
                CreateOctokitOrganization("github")
            }.ToObservable());
            var hostCache = new InMemoryBlobCache();
            await hostCache.InsertObject("user", new CachedAccount(CreateOctokitUser("foo")));
            await hostCache.InsertObject("organizations", new[] { new CachedAccount(CreateOctokitUser("bar")) });
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache, Substitute.For<ITwoFactorChallengeHandler>());

            var cachedAccounts = await host.GetAccounts(Substitute.For<IAvatarProvider>()).FirstAsync();
            var fetchedAccounts = await host.GetAccounts(Substitute.For<IAvatarProvider>()).LastAsync();

            Assert.Equal(2, cachedAccounts.Count);
            Assert.Equal("foo", cachedAccounts[0].Login);
            Assert.Equal("bar", cachedAccounts[1].Login);

            Assert.Equal(4, fetchedAccounts.Count);
            Assert.Equal("lagavulin", fetchedAccounts[0].Login);
            Assert.Equal("illuminati", fetchedAccounts[1].Login);
            Assert.Equal("islay", fetchedAccounts[2].Login);
            Assert.Equal("github", fetchedAccounts[3].Login);
        }

        [Fact]
        public async Task ReturnsUsersAndOrganizationsFromMixedCacheAndFetch()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            apiClient.GetUser().Returns(Observable.Return(CreateOctokitUser("lagavulin")));
            apiClient.GetOrganizations().Returns(new Organization[]
            {
                CreateOctokitOrganization("illuminati"),
                CreateOctokitOrganization("islay"),
                CreateOctokitOrganization("github")
            }.ToObservable());
            var hostCache = new InMemoryBlobCache();
            await hostCache.InsertObject("user", new CachedAccount(CreateOctokitUser("foo")));
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache, Substitute.For<ITwoFactorChallengeHandler>());

            var accounts = await host.GetAccounts(Substitute.For<IAvatarProvider>());

            Assert.Equal(4, accounts.Count);
            Assert.Equal("foo", accounts[0].Login);
            Assert.Equal("illuminati", accounts[1].Login);
            Assert.Equal("islay", accounts[2].Login);
            Assert.Equal("github", accounts[3].Login);
        }
    }

    static User CreateOctokitUser(string login)
    {
        return new User("https://url", "", "", 1, "GitHub", DateTimeOffset.UtcNow, 0, "email", 100, 100, true, "http://url", 10, 42, "somewhere", login, "Who cares", 1, new Plan(), 1, 1, 1, "https://url", false);
    }

    static Organization CreateOctokitOrganization(string login)
    {
        return new Organization("https://url", "", "", 1, "GitHub", DateTimeOffset.UtcNow, 0, "email", 100, 100, true, "http://url", 10, 42, "somewhere", login, "Who cares", 1, new Plan(), 1, 1, 1, "https://url", "billing");
    }
}
