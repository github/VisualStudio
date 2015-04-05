using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using NSubstitute;
using Octokit;
using Xunit;

public class HostCacheTests
{
    public class TheGetUserMethod
    {
        [Fact]
        public async Task RetrievesUserFromCache()
        {
            var userAccountCache = new InMemoryBlobCache();
            await userAccountCache.InsertObject("user", new CachedAccount { Login = "hongkongfuey" });
            var apiClient = Substitute.For<IApiClient>();
            var hostCache = new HostCache(userAccountCache, apiClient);

            var user = await hostCache.GetAndFetchUser();

            Assert.Equal("hongkongfuey", user.Login);
        }

        [Fact]
        public async Task UpdatesUserFromApi()
        {
            var userAccountCache = new InMemoryBlobCache();
            await userAccountCache.InsertObject("user", new CachedAccount { Login = "hongkongfuey" });
            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetUser().Returns(Observable.Return(CreateOctokitUser("snoopy")));
            var hostCache = new HostCache(userAccountCache, apiClient);

            var cachedUser = await hostCache.GetAndFetchUser().FirstAsync();
            var fetchedUser = await hostCache.GetAndFetchUser().LastAsync();

            Assert.Equal("hongkongfuey", cachedUser.Login);
            Assert.Equal("snoopy", fetchedUser.Login);
        }
    }

    static User CreateOctokitUser(string login)
    {
        return new User("https://url", "", "", 1, "GitHub", DateTimeOffset.UtcNow, 0, "email", 100, 100, true, "http://url", 10, 42, "somewhere", login, "Who cares", 1, new Plan(), 1, 1, 1, "https://url", false);
    }
}
