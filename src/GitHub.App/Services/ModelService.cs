using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using NLog;
using NullGuard;
using Octokit;
using GitHub.Primitives;
using System.Collections.ObjectModel;

namespace GitHub.Services
{
    [Export(typeof(IModelService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ModelService : IModelService
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IApiClient apiClient;
        readonly IBlobCache hostCache;
        readonly IAvatarProvider avatarProvider;

        public ModelService(IApiClient apiClient, IBlobCache hostCache, IAvatarProvider avatarProvider)
        {
            this.apiClient = apiClient;
            this.hostCache = hostCache;
            this.avatarProvider = avatarProvider;
        }

        public IObservable<IReadOnlyList<GitIgnoreItem>> GetGitIgnoreTemplates()
        {
            return Observable.Defer(() =>
                hostCache.GetAndRefreshObject(
                    "gitignores",
                    GetOrderedGitIgnoreTemplatesFromApi,
                    TimeSpan.FromDays(1),
                    TimeSpan.FromDays(7))
                .ToReadOnlyList(GitIgnoreItem.Create, GitIgnoreItem.None))
                .Catch<IReadOnlyList<GitIgnoreItem>, Exception>(e =>
                {
                    log.Info("Failed to retrieve GitIgnoreTemplates", e);
                    return Observable.Return(new[] { GitIgnoreItem.None });
                });
        }

        public IObservable<IReadOnlyList<LicenseItem>> GetLicenses()
        {
            return Observable.Defer(() =>
                hostCache.GetAndRefreshObject(
                    "licenses",
                    GetOrderedLicensesFromApi,
                    TimeSpan.FromDays(1),
                    TimeSpan.FromDays(7))
                .ToReadOnlyList(Create, LicenseItem.None))
                .Catch<IReadOnlyList<LicenseItem>, Exception>(e =>
                {
                    log.Info("Failed to retrieve GitIgnoreTemplates", e);
                    return Observable.Return(new[] { LicenseItem.None });
                });
        }

        public IObservable<IReadOnlyList<IAccount>> GetAccounts()
        {
            return Observable.Zip(
                GetUser(),
                GetUserOrganizations(),
                (user, orgs) => user.Concat(orgs))
            .ToReadOnlyList(Create);
        }

        IObservable<IEnumerable<LicenseCacheItem>> GetOrderedLicensesFromApi()
        {
            return apiClient.GetLicenses()
                .WhereNotNull()
                .Select(LicenseCacheItem.Create)
                .ToList()
                .Select(licenses => licenses.OrderByDescending(lic => LicenseItem.IsRecommended(lic.Key)));
        }

        IObservable<IEnumerable<string>> GetOrderedGitIgnoreTemplatesFromApi()
        {
            return apiClient.GetGitIgnoreTemplates()
                .WhereNotNull()
                .ToList()
                .Select(templates => templates.OrderByDescending(GitIgnoreItem.IsRecommended));
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUser()
        {
            return hostCache.GetAndRefreshObject("user",
                () => apiClient.GetUser().Select(AccountCacheItem.Create), TimeSpan.FromMinutes(5), TimeSpan.FromDays(7))
                .Take(1)
                .ToList();
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUserOrganizations()
        {
            return GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(user.Login + "|orgs",
                    () => apiClient.GetOrganizations().Select(AccountCacheItem.Create).ToList(),
                    TimeSpan.FromMinutes(5), TimeSpan.FromDays(7)))
                .Catch<IEnumerable<AccountCacheItem>, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        log.Error("Retrieve user organizations failed because user is not stored in the cache.", (Exception)e);
                        return Observable.Return(Enumerable.Empty<AccountCacheItem>());
                    })
                 .Catch<IEnumerable<AccountCacheItem>, Exception>(e =>
                 {
                     log.Error("Retrieve user organizations failed.", e);
                     return Observable.Return(Enumerable.Empty<AccountCacheItem>());
                 });
        }

        public IObservable<IReadOnlyList<IRepositoryModel>> GetRepositories()
        {
            return GetUserRepositories(RepositoryType.Owner)
                .Take(1)
                .Concat(GetUserRepositories(RepositoryType.Member).Take(1))
                .Concat(GetAllRepositoriesForAllOrganizations());
        }

        public IObservable<AccountCacheItem> GetUserFromCache()
        {
            return Observable.Defer(() => hostCache.GetObject<AccountCacheItem>("user"));
        }

        public IObservable<Unit> InvalidateAll()
        {
            return hostCache.InvalidateAll().ContinueAfter(() => hostCache.Vacuum());
        }

        IObservable<IReadOnlyList<IRepositoryModel>> GetUserRepositories(RepositoryType repositoryType)
        {
            return Observable.Defer(() => GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(string.Format(CultureInfo.InvariantCulture, "{0}|{1}:repos", user.Login, repositoryType),
                    () => GetUserRepositoriesFromApi(repositoryType),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromDays(7)))
                .ToReadOnlyList(Create))
                .Catch<IReadOnlyList<IRepositoryModel>, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        string message = string.Format(CultureInfo.InvariantCulture,
                            "Retrieving '{0}' user repositories failed because user is not stored in the cache.",
                            repositoryType);
                        log.Error(message, e);
                        return Observable.Return(new IRepositoryModel[] { });
                    });
        }

        IObservable<IEnumerable<RepositoryCacheItem>> GetUserRepositoriesFromApi(RepositoryType repositoryType)
        {
            return apiClient.GetUserRepositories(repositoryType)
                .WhereNotNull()
                .Select(RepositoryCacheItem.Create)
                .ToList()
                .Catch<IEnumerable<RepositoryCacheItem>, Exception>(_ => Observable.Return(Enumerable.Empty<RepositoryCacheItem>()));
        }

        IObservable<IReadOnlyList<IRepositoryModel>> GetAllRepositoriesForAllOrganizations()
        {
            return GetUserOrganizations()
                .SelectMany(org => org.ToObservable())
                .SelectMany(org => GetOrganizationRepositories(org.Login).Take(1));
        }

        IObservable<IReadOnlyList<IRepositoryModel>> GetOrganizationRepositories(string organization)
        {
            return Observable.Defer(() => GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(string.Format(CultureInfo.InvariantCulture, "{0}|{1}|repos", user.Login, organization),
                    () => apiClient.GetRepositoriesForOrganization(organization)
                                   .Select(RepositoryCacheItem.Create)
                                   .ToList(),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromDays(7)))
                .ToReadOnlyList(Create))
                .Catch<IReadOnlyList<IRepositoryModel>, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        string message = string.Format(
                            CultureInfo.InvariantCulture,
                            "Retrieving '{0}' org repositories failed because user is not stored in the cache.",
                            organization);
                        log.Error(message, e);
                        return Observable.Return(new IRepositoryModel[] { });
                    });
        }

        public IObservable<IReadOnlyList<IPullRequestModel>> GetPullRequests(ISimpleRepositoryModel repo)
        {
            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}|pr", user.Login, repo.Name))
                .Replay()
                .RefCount();

            // Since the api to list pull requests returns all the data for each pr, cache each pr in its own entry
            // and also cache an index that contains all the keys for each pr. This way we can fetch prs in bulk
            // but also individually without duplicating information.
            var data = keyobs
                .SelectMany(key =>
                    hostCache.GetOrCreateObject(key, () => CacheIndex.Create(key))
                    .Concat(
                        apiClient.GetPullRequestsForRepository(repo.CloneUrl.Owner, repo.CloneUrl.RepositoryName)
                            .Select(PullRequestCacheItem.Create)
                            .Do(pr => hostCache.InsertObject(string.Format(CultureInfo.InvariantCulture, "{0}|{1}", key, pr.Number), pr))
                            .SelectMany(pr => CacheIndex.AddAndSave(hostCache, key, pr.Number, pr.UpdatedAt))
                            //.Buffer(100)
                            //.Select(buffer => buffer.ToDictionary(pr => string.Format(CultureInfo.InvariantCulture, "{0}|{1}", key, pr.Number)))
                            //.Do(list => hostCache.InsertObjects(list))
                            //.SelectMany(pr => CacheIndex.AddAndSave(hostCache, key, pr.Values.Select(x => x.Number), pr.Values.Select(x => x.UpdatedAt)))
                    )
                    .Publish()
                    .RefCount());

            return data
                .SelectMany(index => hostCache.GetObjects<PullRequestCacheItem>(index.Keys))
                .Select(x => new ReadOnlyCollection<IPullRequestModel>(x.Values.Select(Create).ToList()))
                .Catch<IReadOnlyList<IPullRequestModel>, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        string message = string.Format(
                            CultureInfo.InvariantCulture,
                            "Retrieving '{0}' pull requests failed because user is not stored in the cache.",
                            repo.Name);
                        log.Error(message, e);
                        return Observable.Return(new IPullRequestModel[] { });
                    });
        }

        static LicenseItem Create(LicenseCacheItem licenseCacheItem)
        {
            return new LicenseItem(licenseCacheItem.Key, licenseCacheItem.Name);
        }

        Models.Account Create(AccountCacheItem accountCacheItem)
        {
            return new Models.Account(
                accountCacheItem.Login,
                accountCacheItem.IsUser,
                accountCacheItem.IsEnterprise,
                accountCacheItem.OwnedPrivateRepositoriesCount,
                accountCacheItem.PrivateRepositoriesInPlanCount,
                avatarProvider.GetAvatar(accountCacheItem));
        }

        IRepositoryModel Create(RepositoryCacheItem repositoryCacheItem)
        {
            return new RepositoryModel(
                repositoryCacheItem.Name,
                new UriString(repositoryCacheItem.CloneUrl),
                repositoryCacheItem.Private,
                repositoryCacheItem.Fork,
                Create(repositoryCacheItem.Owner));
        }

        IPullRequestModel Create(PullRequestCacheItem prCacheItem)
        {
            return new PullRequestModel()
            {
                Title = prCacheItem.Title,
                Number = prCacheItem.Number,
                CreatedAt = prCacheItem.CreatedAt,
                Author = Create(prCacheItem.Author),
                CommentCount = prCacheItem.CommentCount
            };
        }

        public IObservable<Unit> InsertUser(AccountCacheItem user)
        {
            return hostCache.InsertObject("user", user);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                try
                {
                    hostCache.Dispose();
                }
                catch (Exception e)
                {
                    log.Warn("Exception occured while disposing host cache", e);
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public class LicenseCacheItem
        {
            public static LicenseCacheItem Create(LicenseMetadata licenseMetadata)
            {
                return new LicenseCacheItem { Key = licenseMetadata.Key, Name = licenseMetadata.Name };
            }

            public string Key { get; set; }
            public string Name { get; set; }
        }

        public class RepositoryCacheItem
        {
            public static RepositoryCacheItem Create(Repository apiRepository)
            {
                return new RepositoryCacheItem(apiRepository);
            }

            public RepositoryCacheItem()
            {}

            public RepositoryCacheItem(Repository apiRepository)
            {
                Name = apiRepository.Name;
                Owner = AccountCacheItem.Create(apiRepository.Owner);
                CloneUrl = apiRepository.CloneUrl;
                Private = apiRepository.Private;
                Fork = apiRepository.Fork;
            }

            public string Name { get; set; }
            [AllowNull]
            public AccountCacheItem Owner
            {
                [return: AllowNull]
                get; set;
            }
            public string CloneUrl { get; set; }
            public bool Private { get; set; }
            public bool Fork { get; set; }
        }

        public class PullRequestCacheItem
        {
            public static PullRequestCacheItem Create(PullRequest pr)
            {
                return new PullRequestCacheItem(pr);
            }

            public PullRequestCacheItem() {}
            public PullRequestCacheItem(PullRequest pr)
            {
                Title = pr.Title;
                Number = pr.Number;
                CommentCount = pr.Comments;
                Author = new AccountCacheItem(pr.User);
                CreatedAt = pr.CreatedAt;
                UpdatedAt = pr.UpdatedAt;
            }

            [AllowNull]
            public string Title { [return:AllowNull] get; set; }
            public int Number { get; set; }
            public int CommentCount { get; set; }
            [AllowNull]
            public AccountCacheItem Author { [return:AllowNull] get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }

        public class CacheIndex
        {
            public static CacheIndex Create(string key)
            {
                return new CacheIndex() { IndexKey = key };
            }

            public CacheIndex()
            {
                Keys = new List<string>();
            }

            public static IObservable<CacheIndex> AddAndSave(IBlobCache cache, string indexKey, int key, DateTimeOffset lastUpdated)
            {
                return cache.GetObject<CacheIndex>(indexKey)
                    .Do(index =>
                    {
                        var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", index.IndexKey, key);
                        if (!index.Keys.Contains(k))
                            index.Keys.Add(k);
                        index.UpdatedAt = lastUpdated > index.UpdatedAt ? lastUpdated : index.UpdatedAt;
                    })
                    .SelectMany(index => cache.InsertObject(index.IndexKey, index).Select(x => index));
            }

            public static IObservable<CacheIndex> AddAndSave(IBlobCache cache, string indexKey, IEnumerable<int> keys, IEnumerable<DateTimeOffset> lastUpdates)
            {
                return cache.GetObject<CacheIndex>(indexKey)
                    .Do(index =>
                    {
                        foreach (var key in keys.Select(x => string.Format(CultureInfo.InvariantCulture, "{0}|{1}", index.IndexKey, x)))
                        {
                            if (!index.Keys.Contains(key))
                                index.Keys.Add(key);
                        }
                        foreach (var lastUpdated in lastUpdates)
                            index.UpdatedAt = lastUpdated > index.UpdatedAt ? lastUpdated : index.UpdatedAt;
                    })
                    .SelectMany(index => cache.InsertObject(index.IndexKey, index).Select(x => index));
            }

            [AllowNull]
            public string IndexKey { [return:AllowNull] get; set; }
            public List<string> Keys { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }
    }
}
