using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Services;
using NSubstitute;
using Octokit;
using NUnit.Framework;
using System.Globalization;
using System.Reactive.Subjects;
using System.Threading;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Collections;
using ReactiveUI;
using static GitHub.Services.ModelService;

public class ModelServiceTests
{
    const int Timeout = 2000;
    public class TheGetCurrentUserMethod : TestBaseClass
    {
        [Test]
        public async Task RetrievesCurrentUserAsync()
        {
            var cache = new InMemoryBlobCache();
            await cache.InsertObject<AccountCacheItem>("user", new AccountCacheItem(CreateOctokitUser("octocat")));
            var modelService = CreateTarget(hostCache: cache);

            var user = await modelService.GetCurrentUser();

            Assert.That("octocat", Is.EqualTo(user.Login));
        }
    }

    public class TheInsertUserMethod : TestBaseClass
    {
        [Test]
        public async Task AddsUserToCacheAsync()
        {
            var cache = new InMemoryBlobCache();
            var modelService = CreateTarget(hostCache: cache);

            var user = await modelService.InsertUser(new AccountCacheItem(CreateOctokitUser("octocat")));

            var cached = await cache.GetObject<AccountCacheItem>("user");
            Assert.That("octocat", Is.EqualTo(cached.Login));
        }
    }

    public class TheGetGitIgnoreTemplatesMethod : TestBaseClass
    {
        [Test]
        public async Task CanRetrieveAndCacheGitIgnoresAsync()
        {
            var data = new[] { "dotnet", "peanuts", "bloomcounty" };
            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetGitIgnoreTemplates().Returns(data.ToObservable());
            var cache = new InMemoryBlobCache();
            var modelService = CreateTarget(apiClient: apiClient, hostCache: cache);

            var fetched = await modelService.GetGitIgnoreTemplates().ToList();

            Assert.That(3, Is.EqualTo(fetched.Count));
            for (int i = 0; i < data.Length; i++)
                Assert.That(data[i], Is.EqualTo(fetched[i].Name));

            var indexKey = CacheIndex.GitIgnoresPrefix;
            var cached = await cache.GetObject<CacheIndex>(indexKey);
            Assert.That(3, Is.EqualTo(cached.Keys.Count));

            var items = await cache.GetObjects<GitIgnoreCacheItem>(cached.Keys).Take(1);
            for (int i = 0; i < data.Length; i++)
                Assert.That(data[i], Is.EqualTo(items[indexKey + "|" + data[i]].Name));
        }
    }

    public class TheGetLicensesMethod : TestBaseClass
    {
        [Test]
        public async Task CanRetrieveAndCacheLicensesAsync()
        {
            var data = new[]
            {
                new LicenseMetadata("mit", null, "MIT", "foo", "https://github.com/", false),
                new LicenseMetadata("apache", null, "Apache", "foo", "https://github.com/", false)
            };

            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetLicenses().Returns(data.ToObservable());
            var cache = new InMemoryBlobCache();
            var modelService = CreateTarget(apiClient: apiClient, hostCache: cache);

            var fetched = await modelService.GetLicenses().ToList();

            Assert.That(2, Is.EqualTo(fetched.Count));
            for (int i = 0; i < data.Length; i++)
                Assert.That(data[i].Name, Is.EqualTo(fetched[i].Name));

            var indexKey = CacheIndex.LicensesPrefix;
            var cached = await cache.GetObject<CacheIndex>(indexKey);
            Assert.That(2, Is.EqualTo(cached.Keys.Count));

            var items = await cache.GetObjects<LicenseCacheItem>(cached.Keys).Take(1);
            for (int i = 0; i < data.Length; i++)
                Assert.That(data[i].Name, Is.EqualTo(items[indexKey + "|" + data[i].Key].Name));
        }

        [Test]
        public async Task ReturnsEmptyIfLicenseApiNotFoundAsync()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetLicenses()
                .Returns(Observable.Throw<LicenseMetadata>(new NotFoundException("Not Found", HttpStatusCode.NotFound)));
            var modelService = CreateTarget(apiClient: apiClient);

            var fetched = await modelService.GetLicenses().ToList();

            Assert.That(0, Is.EqualTo(fetched.Count));
        }

        [Test]
        public async Task ReturnsEmptyIfCacheReadFailsAsync()
        {
            var apiClient = Substitute.For<IApiClient>();
            var cache = Substitute.For<IBlobCache>();
            cache.Get(Args.String)
                .Returns(Observable.Throw<byte[]>(new InvalidOperationException("Unknown")));
            var modelService = CreateTarget(apiClient: apiClient, hostCache: cache);

            var fetched = await modelService.GetLicenses().ToList();

            Assert.That(0, Is.EqualTo(fetched.Count));
        }
    }

    public class TheGetAccountsMethod : TestBaseClass
    {
        [Test]
        [Ignore("Skip this test as it will no longer be relevant with the GraphQL migration")]
        public async Task CanRetrieveAndCacheUserAndAccountsAsync()
        {
            var orgs = new[]
            {
                CreateOctokitOrganization("github"),
                CreateOctokitOrganization("fake")
            };
            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetUser().Returns(Observable.Return(CreateOctokitUser("snoopy")));
            apiClient.GetOrganizations().Returns(orgs.ToObservable());
            var cache = new InMemoryBlobCache();
            var modelService = CreateTarget(apiClient: apiClient, hostCache: cache);
            await modelService.InsertUser(new AccountCacheItem { Login = "snoopy" });

            var fetched = await modelService.GetAccounts();

            Assert.That(3, Is.EqualTo(fetched.Count));
            Assert.That("snoopy", Is.EqualTo(fetched[0].Login));
            Assert.That("github", Is.EqualTo(fetched[1].Login));
            Assert.That("fake", Is.EqualTo(fetched[2].Login));
            var cachedOrgs = await cache.GetObject<IReadOnlyList<AccountCacheItem>>("snoopy|orgs");
            Assert.That(2, Is.EqualTo(cachedOrgs.Count));
            Assert.That("github", Is.EqualTo(cachedOrgs[0].Login));
            Assert.That("fake", Is.EqualTo(cachedOrgs[1].Login));
            var cachedUser = await cache.GetObject<AccountCacheItem>("user");
            Assert.That("snoopy", Is.EqualTo(cachedUser.Login));
        }

        [Test]
        [Ignore("Skip this test as it will no longer be relevant with the GraphQL migration")]
        public async Task CanRetrieveUserFromCacheAndAccountsFromApiAsync()
        {
            var orgs = new[]
            {
                CreateOctokitOrganization("github"),
                CreateOctokitOrganization("fake")
            };
            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetOrganizations().Returns(orgs.ToObservable());
            var cache = new InMemoryBlobCache();
            var modelService = CreateTarget(apiClient: apiClient, hostCache: cache);
            await modelService.InsertUser(new AccountCacheItem(CreateOctokitUser("octocat")));

            var fetched = await modelService.GetAccounts();

            Assert.That(3, Is.EqualTo(fetched.Count));
            Assert.That("octocat", Is.EqualTo(fetched[0].Login));
            Assert.That("github", Is.EqualTo(fetched[1].Login));
            Assert.That("fake", Is.EqualTo(fetched[2].Login));
            var cachedOrgs = await cache.GetObject<IReadOnlyList<AccountCacheItem>>("octocat|orgs");
            Assert.That(2, Is.EqualTo(cachedOrgs.Count));
            Assert.That("github", Is.EqualTo(cachedOrgs[0].Login));
            Assert.That("fake", Is.EqualTo(cachedOrgs[1].Login));
            var cachedUser = await cache.GetObject<AccountCacheItem>("user");
            Assert.That("octocat", Is.EqualTo(cachedUser.Login));
        }

        [Test]
        public async Task OnlyRetrievesOneUserEvenIfCacheOrApiReturnsMoreThanOneAsync()
        {
            // This should be impossible, but let's pretend it does happen.
            var users = new[]
            {
                CreateOctokitUser("peppermintpatty"),
                CreateOctokitUser("peppermintpatty")
            };
            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetUser().Returns(users.ToObservable());
            apiClient.GetOrganizations().Returns(Observable.Empty<Organization>());
            var modelService = CreateTarget(apiClient: apiClient);

            var fetched = await modelService.GetAccounts();

            Assert.That(1, Is.EqualTo(fetched.Count));
            Assert.That("peppermintpatty", Is.EqualTo(fetched[0].Login));
        }
    }

    public class TheGetRepositoriesMethod : TestBaseClass
    {
        [Test]
        [Ignore("Skip this test as it will no longer be relevant with the GraphQL migration")]
        public async Task CanRetrieveAndCacheRepositoriesForUserAndOrganizationsAsync()
        {
            var orgs = new[]
            {
                CreateOctokitOrganization("github"),
                CreateOctokitOrganization("octokit")
            };
            var ownedRepos = new[]
            {
                CreateRepository("haacked", "seegit"),
                CreateRepository("haacked", "codehaacks")
            };
            var memberRepos = new[]
            {
                CreateRepository("mojombo", "semver"),
                CreateRepository("ninject", "ninject"),
                CreateRepository("jabbr", "jabbr"),
                CreateRepository("fody", "nullguard")
            };
            var githubRepos = new[]
            {
                CreateRepository("github", "visualstudio")
            };
            var octokitRepos = new[]
            {
                CreateRepository("octokit", "octokit.net"),
                CreateRepository("octokit", "octokit.rb"),
                CreateRepository("octokit", "octokit.objc")
            };
            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetOrganizations().Returns(orgs.ToObservable());
            apiClient.GetUserRepositories(RepositoryType.Owner).Returns(ownedRepos.ToObservable());
            apiClient.GetUserRepositories(RepositoryType.Member).Returns(memberRepos.ToObservable());
            apiClient.GetRepositoriesForOrganization("github").Returns(githubRepos.ToObservable());
            apiClient.GetRepositoriesForOrganization("octokit").Returns(octokitRepos.ToObservable());
            var cache = new InMemoryBlobCache();
            var modelService = CreateTarget(apiClient: apiClient, hostCache: cache);
            await modelService.InsertUser(new AccountCacheItem { Login = "opus" });

            var fetched = await modelService.GetRepositories().ToList();

            Assert.That(4, Is.EqualTo(fetched.Count));
            Assert.That(2, Is.EqualTo(fetched[0].Count));
            Assert.That(4, Is.EqualTo(fetched[1].Count));
            Assert.That(1, Is.EqualTo(fetched[2].Count));
            Assert.That(3, Is.EqualTo(fetched[3].Count));
            Assert.That("seegit", Is.EqualTo(fetched[0][0].Name));
            Assert.That("codehaacks", Is.EqualTo(fetched[0][1].Name));
            Assert.That("semver", Is.EqualTo(fetched[1][0].Name));
            Assert.That("ninject", Is.EqualTo(fetched[1][1].Name));
            Assert.That("jabbr", Is.EqualTo(fetched[1][2].Name));
            Assert.That("nullguard", Is.EqualTo(fetched[1][3].Name));
            Assert.That("visualstudio", Is.EqualTo(fetched[2][0].Name));
            Assert.That("octokit.net", Is.EqualTo(fetched[3][0].Name));
            Assert.That("octokit.rb", Is.EqualTo(fetched[3][1].Name));
            Assert.That("octokit.objc", Is.EqualTo(fetched[3][2].Name));
            var cachedOwnerRepositories = await cache.GetObject<IReadOnlyList<ModelService.RepositoryCacheItem>>("opus|Owner:repos");
            Assert.That(2, Is.EqualTo(cachedOwnerRepositories.Count));
            Assert.That("seegit", Is.EqualTo(cachedOwnerRepositories[0].Name));
            Assert.That("haacked", Is.EqualTo(cachedOwnerRepositories[0].Owner.Login));
            Assert.That("codehaacks", Is.EqualTo(cachedOwnerRepositories[1].Name));
            Assert.That("haacked", Is.EqualTo(cachedOwnerRepositories[1].Owner.Login));
            var cachedMemberRepositories = await cache.GetObject<IReadOnlyList<ModelService.RepositoryCacheItem>>("opus|Member:repos");
            Assert.That(4, Is.EqualTo(cachedMemberRepositories.Count));
            Assert.That("semver", Is.EqualTo(cachedMemberRepositories[0].Name));
            Assert.That("mojombo", Is.EqualTo(cachedMemberRepositories[0].Owner.Login));
            Assert.That("ninject", Is.EqualTo(cachedMemberRepositories[1].Name));
            Assert.That("ninject", Is.EqualTo(cachedMemberRepositories[1].Owner.Login));
            Assert.That("jabbr", Is.EqualTo(cachedMemberRepositories[2].Name));
            Assert.That("jabbr", Is.EqualTo(cachedMemberRepositories[2].Owner.Login));
            Assert.That("nullguard", Is.EqualTo(cachedMemberRepositories[3].Name));
            Assert.That("fody", Is.EqualTo(cachedMemberRepositories[3].Owner.Login));
            var cachedGitHubRepositories = await cache.GetObject<IReadOnlyList<ModelService.RepositoryCacheItem>>("opus|github|repos");
            Assert.That(1, Is.EqualTo(cachedGitHubRepositories.Count));
            Assert.That("seegit", Is.EqualTo(cachedOwnerRepositories[0].Name));
            Assert.That("haacked", Is.EqualTo(cachedOwnerRepositories[0].Owner.Login));
            Assert.That("codehaacks", Is.EqualTo(cachedOwnerRepositories[1].Name));
            Assert.That("haacked", Is.EqualTo(cachedOwnerRepositories[1].Owner.Login));
            var cachedOctokitRepositories = await cache.GetObject<IReadOnlyList<ModelService.RepositoryCacheItem>>("opus|octokit|repos");
            Assert.That("octokit.net", Is.EqualTo(cachedOctokitRepositories[0].Name));
            Assert.That("octokit", Is.EqualTo(cachedOctokitRepositories[0].Owner.Login));
            Assert.That("octokit.rb", Is.EqualTo(cachedOctokitRepositories[1].Name));
            Assert.That("octokit", Is.EqualTo(cachedOctokitRepositories[1].Owner.Login));
            Assert.That("octokit.objc", Is.EqualTo(cachedOctokitRepositories[2].Name));
            Assert.That("octokit", Is.EqualTo(cachedOctokitRepositories[2].Owner.Login));
        }

        [Test]
        public async Task WhenNotLoggedInReturnsEmptyCollectionAsync()
        {
            var apiClient = Substitute.For<IApiClient>();
            var modelService = CreateTarget(apiClient: apiClient);

            var repos = await modelService.GetRepositories();

            Assert.That(0, Is.EqualTo(repos.Count));
        }

        [Test]
        public async Task WhenLoggedInDoesNotBlowUpOnUnexpectedNetworkProblemsAsync()
        {
            var apiClient = Substitute.For<IApiClient>();
            var modelService = CreateTarget(apiClient: apiClient);
            apiClient.GetOrganizations()
                .Returns(Observable.Throw<Organization>(new NotFoundException("Not Found", HttpStatusCode.NotFound)));

            var repos = await modelService.GetRepositories();

            Assert.That(0, Is.EqualTo(repos.Count));
        }
    }

    public class TheInvalidateAllMethod : TestBaseClass
    {
        [Test]
        public async Task InvalidatesTheCacheAsync()
        {
            var apiClient = Substitute.For<IApiClient>();
            var cache = new InMemoryBlobCache();
            var modelService = CreateTarget(apiClient: apiClient, hostCache: cache);
            var user = await modelService.InsertUser(new AccountCacheItem(CreateOctokitUser("octocat")));
            //Assert.Single((await cache.GetAllObjects<AccountCacheItem>()));

            await modelService.InvalidateAll();

            //Assert.That((cache.GetAllObjects<AccountCacheItem>(), Is.Empty));
        }

        [Test]
        public async Task VaccumsTheCacheAsync()
        {
            var apiClient = Substitute.For<IApiClient>();
            var cache = Substitute.For<IBlobCache>();
            cache.InvalidateAll().Returns(Observable.Return(Unit.Default));
            var received = false;
            cache.Vacuum().Returns(x =>
            {
                received = true;
                return Observable.Return(Unit.Default);
            });
            var modelService = CreateTarget(apiClient: apiClient, hostCache: cache);

            await modelService.InvalidateAll();
            Assert.True(received);
        }
    }

    static ModelService CreateTarget(
        IApiClient apiClient = null,
        Octokit.IConnection graphql = null,
        IBlobCache hostCache = null,
        IAvatarProvider avatarProvider = null)
    {
        return new ModelService(
            apiClient ?? Substitute.For<IApiClient>(),
            hostCache ?? new InMemoryBlobCache(),
            Substitute.For<IAvatarProvider>());
    }
}
