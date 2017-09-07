using System;
using System.Collections.Generic;
using System.Net;
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
using Xunit;

public class RepositoryHostTests
{
    public class TheLoginMethod : TestBaseClass
    {
        [Fact]
        public async Task LogsTheUserInSuccessfullyAndCachesRelevantInfo()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            apiClient.GetOrCreateApplicationAuthenticationCode(
                Args.TwoFactorChallengCallback, Args.String, Args.Boolean)
                .Returns(Observable.Return(new ApplicationAuthorization("S3CR3TS")));
            apiClient.GetUser().Returns(Observable.Return(CreateOctokitUser("baymax")));
            var hostCache = new InMemoryBlobCache();
            var modelService = new ModelService(apiClient, hostCache, Substitute.For<IAvatarProvider>());
            var loginManager = Substitute.For<ILoginManager>();
            loginManager.Login(HostAddress.GitHubDotComHostAddress, Arg.Any<IGitHubClient>(), "baymax", "aPassword").Returns(CreateOctokitUser("baymax"));
            var keychain = Substitute.For<IKeychain>();
            var usage = Substitute.For<IUsageTracker>();
            var host = new RepositoryHost(apiClient, modelService, loginManager, keychain, usage);

            var result = await host.LogIn("baymax", "aPassword");

            Assert.Equal(AuthenticationResult.Success, result);
            var user = await hostCache.GetObject<AccountCacheItem>("user");
            Assert.NotNull(user);
            Assert.Equal("baymax", user.Login);
        }

        [Fact]
        public async Task IncrementsLoginCount()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            apiClient.GetOrCreateApplicationAuthenticationCode(
                Args.TwoFactorChallengCallback, Args.String, Args.Boolean)
                .Returns(Observable.Return(new ApplicationAuthorization("S3CR3TS")));
            apiClient.GetUser().Returns(Observable.Return(CreateOctokitUser("baymax")));
            var hostCache = new InMemoryBlobCache();
            var modelService = Substitute.For<IModelService>();
            var loginManager = Substitute.For<ILoginManager>();
            loginManager.Login(HostAddress.GitHubDotComHostAddress, Arg.Any<IGitHubClient>(), "baymax", "aPassword").Returns(CreateOctokitUser("baymax"));
            var keychain = Substitute.For<IKeychain>();
            var usage = Substitute.For<IUsageTracker>();
            var host = new RepositoryHost(apiClient, modelService, loginManager, keychain, usage);

            var result = await host.LogIn("baymax", "aPassword");

            await usage.Received().IncrementLoginCount();
        }

        [Fact]
        public async Task DoesNotLogInWhenRetrievingOauthTokenFails()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            var hostCache = new InMemoryBlobCache();
            var modelService = new ModelService(apiClient, hostCache, Substitute.For<IAvatarProvider>());
            var loginManager = Substitute.For<ILoginManager>();
            loginManager.Login(HostAddress.GitHubDotComHostAddress, Arg.Any<IGitHubClient>(), "jiminy", "cricket")
                .Returns<User>(_ => { throw new NotFoundException("", HttpStatusCode.BadGateway); });
            var keychain = Substitute.For<IKeychain>();
            var usage = Substitute.For<IUsageTracker>();
            var host = new RepositoryHost(apiClient, modelService, loginManager, keychain, usage);

            await Assert.ThrowsAsync<NotFoundException>(async () => await host.LogIn("jiminy", "cricket"));

            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await hostCache.GetObject<AccountCacheItem>("user"));
        }
    }

    public class TheLoginFromCacheMethod : TestBaseClass
    {
        [Fact]
        public async Task LogsTheUserInSuccessfullyAndCachesRelevantInfo()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            var hostCache = new InMemoryBlobCache();
            var modelService = new ModelService(apiClient, hostCache, Substitute.For<IAvatarProvider>());
            var loginManager = Substitute.For<ILoginManager>();
            loginManager.LoginFromCache(HostAddress.GitHubDotComHostAddress, Arg.Any<IGitHubClient>()).Returns(CreateOctokitUser("baymax"));
            var keychain = Substitute.For<IKeychain>();
            var usage = Substitute.For<IUsageTracker>();
            var host = new RepositoryHost(apiClient, modelService, loginManager, keychain, usage);

            var result = await host.LogInFromCache();

            Assert.Equal(AuthenticationResult.Success, result);
            var user = await hostCache.GetObject<AccountCacheItem>("user");
            Assert.NotNull(user);
            Assert.Equal("baymax", user.Login);
        }

        [Fact]
        public async Task IncrementsLoginCount()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            var hostCache = new InMemoryBlobCache();
            var modelService = Substitute.For<IModelService>();
            var loginManager = Substitute.For<ILoginManager>();
            loginManager.LoginFromCache(HostAddress.GitHubDotComHostAddress, Arg.Any<IGitHubClient>()).Returns(CreateOctokitUser("baymax"));
            var keychain = Substitute.For<IKeychain>();
            var usage = Substitute.For<IUsageTracker>();
            var host = new RepositoryHost(apiClient, modelService, loginManager, keychain, usage);

            var result = await host.LogInFromCache();

            await usage.Received().IncrementLoginCount();
        }
    }
}
