using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Primitives;
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
            var hostCache = new TestHostCache();
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache);

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
            var userCache = new InMemoryBlobCache();
            var hostCache = new HostCache(new InMemoryBlobCache(), userCache, apiClient);
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache);

            await host.LogIn("aUsername", "aPassword");

            var user = await userCache.GetObject<CachedAccount>("user");
            Assert.NotNull(user);
            Assert.Equal("lagavulin", user.Login);
        }

        [Fact]
        public async Task SetsTheLoggingInBitDuringLogin()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            apiClient.GetOrCreateApplicationAuthenticationCode(Arg.Any<Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>>>(), Args.Boolean)
                .Returns(Observable.Return(new ApplicationAuthorization("S3CR3TS")));
            var getUserSubject = new Subject<User>();
            apiClient.GetUser().Returns(getUserSubject);
            var userCache = new InMemoryBlobCache();
            var hostCache = new HostCache(new InMemoryBlobCache(), userCache, apiClient);
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache);

            var observable = host.LogIn("aUsername", "aPassword");

            Assert.True(host.IsLoggingIn);
            getUserSubject.OnNext(CreateOctokitUser("missyelliot"));
            getUserSubject.OnCompleted();
            await observable;
            Assert.False(host.IsLoggingIn);
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
            var userCache = new InMemoryBlobCache();
            var hostCache = new HostCache(new InMemoryBlobCache(), userCache, apiClient);
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache);

            var accounts = await host.GetAccounts();

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
            var userCache = new InMemoryBlobCache();
            await userCache.InsertObject("user", new CachedAccount(CreateOctokitUser("foo")));
            await userCache.InsertObject("organizations", new[] { new CachedAccount(CreateOctokitUser("bar")) });
            var hostCache = new HostCache(new InMemoryBlobCache(), userCache, apiClient);
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache);

            var cachedAccounts = await host.GetAccounts().FirstAsync();
            var fetchedAccounts = await host.GetAccounts().LastAsync();

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
            var userCache = new InMemoryBlobCache();
            await userCache.InsertObject("user", new CachedAccount(CreateOctokitUser("foo")));
            var hostCache = new HostCache(new InMemoryBlobCache(), userCache, apiClient);
            var loginCache = new TestLoginCache();
            var host = new RepositoryHost(apiClient, hostCache, loginCache);

            var accounts = await host.GetAccounts();

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
