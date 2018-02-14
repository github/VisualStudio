using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Collections;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
using NUnit.Framework;

namespace BlobCacheTests
{
    [TestFixture]
    public class CacheCorruptionTests : TestBaseClass
    {
        [Test]
        public async Task RecoversFromInvalidJsonInIndexEntries()
        {
            int count = 1;
            var octokitRepos = new[]
            {
                CreateRepository("haacked", "seegit", id: count++),
                CreateRepository("haacked", "codehaacks", id: count++),
                CreateRepository("mojombo", "semver", id: count++),
                CreateRepository("ninject", "ninject", id: count++),
                CreateRepository("jabbr", "jabbr", id: count++),
                CreateRepository("fody", "nullguard", id: count++),
                CreateRepository("github", "visualstudio", id: count++),
                CreateRepository("octokit", "octokit.net", id: count++),
                CreateRepository("octokit", "octokit.rb", id: count++),
                CreateRepository("octokit", "octokit.objc", id: count++),
            };
            var expectedRepos = octokitRepos.Select(x => new RemoteRepositoryModel(x) as IRemoteRepositoryModel).ToList();

            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetRepositories().Returns(octokitRepos.ToObservable());

            var cache = new InMemoryBlobCache();
            var aRepoCacheEntry = new ModelService.RepositoryCacheItem();
            var modelService = new ModelService(apiClient, cache, Substitute.For<IAvatarProvider>());

            await modelService.InsertUser(new AccountCacheItem { Login = "opus" });
            await cache.InsertObject(CacheIndex.RepoPrefix + "|opus", aRepoCacheEntry);

            var repositories = new TrackingCollection<IRemoteRepositoryModel>();
            modelService.GetRepositories(repositories);
            repositories.Subscribe();
            await repositories.OriginalCompleted;

            // the first time we do this, it should fetch from the API
            await apiClient.Received().GetRepositories();
            apiClient.ClearReceivedCalls();

            repositories = new TrackingCollection<IRemoteRepositoryModel>();
            modelService.GetRepositories(repositories);
            repositories.Subscribe();
            await repositories.OriginalCompleted;
            // the second time we do this, it should not fetch
            await apiClient.DidNotReceive().GetRepositories();

            Assert.That(expectedRepos, Is.EquivalentTo(repositories));
        }
    }

}
