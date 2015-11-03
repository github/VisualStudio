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
using GitHub.Collections;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using GitHub.Primitives;
using NLog;
using NullGuard;
using Octokit;

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

        public ITrackingCollection<IPullRequestModel> GetPullRequests(ISimpleRepositoryModel repo)
        {
            // Since the api to list pull requests returns all the data for each pr, cache each pr in its own entry
            // and also cache an index that contains all the keys for each pr. This way we can fetch prs in bulk
            // but also individually without duplicating information. We store things in a custom observable collection
            // that checks whether an item is being updated (coming from the live stream after being retrieved from cache)
            // and replaces it instead of appending, so items get refreshed in-place as they come in.

            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}|pr", user.Login, repo.Name));

            var col = new TrackingCollection<IPullRequestModel>();

            var source = Observable.Defer(() => keyobs
                .SelectMany(key =>
                    hostCache.GetAndFetchLatestFromIndex(key, () =>
                        apiClient.GetPullRequestsForRepository(repo.CloneUrl.Owner, repo.CloneUrl.RepositoryName)
                                 .Select(PullRequestCacheItem.Create),
                        item => col.RemoveItem(Create(item)),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromDays(1))
                )
                .Select(Create)
            );

            col.Listen(source);
            return col;
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
                        return Observable.Return(new IRepositoryModel[] {});
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
                    () => apiClient.GetRepositoriesForOrganization(organization).Select(
                        RepositoryCacheItem.Create).ToList(),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromDays(7)))
                .ToReadOnlyList(Create))
                .Catch<IReadOnlyList<IRepositoryModel>, KeyNotFoundException>(
                    // This could in theory happen if we try to call this before the user is logged in.
                    e =>
                    {
                        string message = string.Format(
                            CultureInfo.InvariantCulture,
                            "Retrieveing '{0}' org repositories failed because user is not stored in the cache.",
                            organization);
                        log.Error(message, e);
                        return Observable.Return(new IRepositoryModel[] {});
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
            return new PullRequestModel(
                prCacheItem.Number,
                prCacheItem.Title,
                Create(prCacheItem.Author),
                prCacheItem.CreatedAt,
                prCacheItem.UpdatedAt)
            {
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

            public RepositoryCacheItem() {}

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
            public string Title {[return: AllowNull] get; set; }
            public int Number { get; set; }
            public int CommentCount { get; set; }
            [AllowNull]
            public AccountCacheItem Author {[return: AllowNull] get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }
    }
}
