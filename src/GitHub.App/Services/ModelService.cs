using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
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
using GitHub.Collections;

namespace GitHub.Services
{
    [Export(typeof(IModelService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ModelService : IModelService
    {
        const string USER = "user";

        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IApiClient apiClient;
        readonly IBlobCache hostCache;
        readonly IAvatarProvider avatarProvider;

        readonly IObservable<AccountCacheItem> userObservable;

        public ModelService(IApiClient apiClient, IBlobCache hostCache, IAvatarProvider avatarProvider)
        {
            this.apiClient = apiClient;
            this.hostCache = hostCache;
            this.avatarProvider = avatarProvider;
            userObservable = Observable.Defer(GetUser);
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
                userObservable,
                GetUserOrganizations()
            )
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

        IObservable<AccountCacheItem> GetUser()
        {
            return hostCache.GetAndRefreshObject(USER,
                () => apiClient.GetUser().Select(AccountCacheItem.Create), TimeSpan.FromMinutes(5), TimeSpan.FromDays(7));
        }

        IObservable<AccountCacheItem> GetUserOrganizations()
        {
            return GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(user.Login + "|orgs",
                    () => apiClient.GetOrganizations().Select(AccountCacheItem.Create),
                    TimeSpan.FromMinutes(5), TimeSpan.FromDays(7)))
                .Catch<AccountCacheItem, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        log.Error("Retrieve user organizations failed because user is not stored in the cache.", (Exception)e);
                        return Observable.Empty<AccountCacheItem>();
                    })
                 .Catch<AccountCacheItem, Exception>(e =>
                 {
                     log.Error("Retrieve user organizations failed.", e);
                     return Observable.Empty<AccountCacheItem>();
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
            return userObservable;
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

        public ITrackingCollection<IPullRequestModel> GetPullRequests(ISimpleRepositoryModel repo)
        {
            // Since the api to list pull requests returns all the data for each pr, cache each pr in its own entry
            // and also cache an index that contains all the keys for each pr. This way we can fetch prs in bulk
            // but also individually without duplicating information. We store things in a custom observable collection
            // that checks whether an item is being updated (coming from the live stream after being retrieved from cache)
            // and replaces it instead of appending, so items get refreshed in-place as they come in.

            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}|pr", user.Login, repo.Name));

            var list = Observable.Defer(() => keyobs
                .SelectMany(key =>
                    hostCache.GetAndFetchLatestFromIndex(key, () =>
                        apiClient.GetPullRequestsForRepository(repo.CloneUrl.Owner, repo.CloneUrl.RepositoryName)
                            .Select(PullRequestCacheItem.Create),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromDays(1))
                )
                .Select(Create)
            );
            return new TrackingCollection<IPullRequestModel>(list);

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
            return new PullRequestModel(prCacheItem.Number, prCacheItem.Title, Create(prCacheItem.Author), prCacheItem.CreatedAt, prCacheItem.UpdatedAt)
            {
                CommentCount = prCacheItem.CommentCount
            };
        }

        public IObservable<Unit> InsertUser(AccountCacheItem user)
        {
            return hostCache.InsertObject(USER, user);
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

        public class CacheItem
        {
            [AllowNull]
            public string Key { [return:AllowNull] get; set; }
            public DateTimeOffset Timestamp { get; set; }

            public IObservable<T> Save<T>(IBlobCache cache, string key, DateTimeOffset? absoluteExpiration) where T : CacheItem
            {
                return cache.InsertObject(string.Format(CultureInfo.InvariantCulture, "{0}|{1}", key, Key), this, absoluteExpiration)
                    .Select(_ => this as T);
            }
        }

        public class PullRequestCacheItem : CacheItem
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

                Key = Number.ToString(CultureInfo.InvariantCulture);
                Timestamp = UpdatedAt;
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

            public IObservable<CacheIndex> AddAndSave(IBlobCache cache, string indexKey, CacheItem item,
                DateTimeOffset? absoluteExpiration = null)
            {
                var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", IndexKey, item.Key);
                if (!Keys.Contains(k))
                    Keys.Add(k);
                UpdatedAt = DateTimeOffset.Now;
                return cache.InsertObject(IndexKey, this, absoluteExpiration)
                            .Select(x => this);
            }

            public static IObservable<CacheIndex> AddAndSaveToIndex(IBlobCache cache, string indexKey, CacheItem item,
                DateTimeOffset? absoluteExpiration = null)
            {
                return cache.GetObject<CacheIndex>(indexKey)
                    .Do(index =>
                    {
                        var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", index.IndexKey, item.Key);
                        if (!index.Keys.Contains(k))
                            index.Keys.Add(k);
                        index.UpdatedAt = DateTimeOffset.Now;
                    })
                    .SelectMany(index => cache.InsertObject(index.IndexKey, index, absoluteExpiration)
                    .Select(x => index));
            }

            [AllowNull]
            public string IndexKey { [return:AllowNull] get; set; }
            public List<string> Keys { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }
    }

    static class CacheIndexExtensions
    {
        public static IObservable<T> GetAndFetchLatestFromIndex<T>(
            this IBlobCache blobCache,
            string key,
            Func<IObservable<T>> fetchFunc,
            TimeSpan refreshInterval,
            TimeSpan maxCacheDuration)
                where T : ModelService.CacheItem
        {
            return Observable.Defer(() =>
            {
                var absoluteExpiration = blobCache.Scheduler.Now + maxCacheDuration;

                try
                {
                    return blobCache.GetAndFetchLatestFromIndex(
                        key,
                        fetchFunc,
                        createdAt => IsExpired(blobCache, createdAt, refreshInterval),
                        absoluteExpiration);
                }
                catch (Exception exc)
                {
                    return Observable.Throw<T>(exc);
                }
            });
        }

        static IObservable<T> GetAndFetchLatestFromIndex<T>(this IBlobCache This,
            string key,
            Func<IObservable<T>> fetchFunc,
            Func<DateTimeOffset, bool> fetchPredicate = null,
            DateTimeOffset? absoluteExpiration = null,
            bool shouldInvalidateOnError = false)
                where T : ModelService.CacheItem
        {
            var fetch = Observable.Defer(() => This.GetOrCreateObject(key, () => ModelService.CacheIndex.Create(key))
                .Select(x => Tuple.Create(x, fetchPredicate == null || x == null || !x.Keys.Any() || fetchPredicate(x.UpdatedAt)))
                .Where(predicateIsTrue => predicateIsTrue.Item2)
                .Select(x => x.Item1)
                .SelectMany(index =>
                {
                    var fetchObs = fetchFunc().Catch<T, Exception>(ex =>
                    {
                        var shouldInvalidate = shouldInvalidateOnError ?
                            This.InvalidateObject<ModelService.CacheIndex>(key) :
                            Observable.Return(Unit.Default);
                        return shouldInvalidate.SelectMany(__ => Observable.Throw<T>(ex));
                    });

                    return fetchObs
                        .SelectMany(x => x.Save<T>(This, key, absoluteExpiration))
                        .Do(x => index.AddAndSave(This, key, x, absoluteExpiration));
                }));

            var cache = Observable.Defer(() => This.GetOrCreateObject(key, () => ModelService.CacheIndex.Create(key))
                .SelectMany(index => This.GetObjects<T>(index.Keys))
                .SelectMany(dict => dict.Values)
                .Do(x => (x as ModelService.PullRequestCacheItem).Title = (x as ModelService.PullRequestCacheItem).Title + " Cached!")
                )
                ;

            return cache.Merge(fetch).Replay().RefCount();
        }

        static bool IsExpired(IBlobCache blobCache, DateTimeOffset itemCreatedAt, TimeSpan cacheDuration)
        {
            var now = blobCache.Scheduler.Now;
            var elapsed = now - itemCreatedAt;

            return elapsed > cacheDuration;
        }

    }
}
