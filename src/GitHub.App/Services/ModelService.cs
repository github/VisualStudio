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

        public IObservable<GitIgnoreItem> GetGitIgnoreTemplates()
        {
            return Observable.Defer(() =>
                hostCache.GetAndFetchLatestFromIndex(CacheIndex.GitIgnoresPrefix, () =>
                        GetGitIgnoreTemplatesFromApi(),
                        item => { },
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromDays(7))
                )
                .Select(Create)
                .Catch<GitIgnoreItem, Exception>(e =>
                {
                    log.Info("Failed to retrieve GitIgnoreTemplates", e);
                    return Observable.Empty<GitIgnoreItem>();
                });
        }

        public IObservable<LicenseItem> GetLicenses()
        {
            return Observable.Defer(() =>
                hostCache.GetAndFetchLatestFromIndex(CacheIndex.LicensesPrefix, () =>
                        GetLicensesFromApi(),
                        item => { },
                        TimeSpan.FromMinutes(1),
                        TimeSpan.FromDays(7))
                )
                .Select(Create)
                .Catch<LicenseItem, Exception>(e =>
                {
                    log.Info("Failed to retrieve licenses", e);
                    return Observable.Empty<LicenseItem>();
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

        IObservable<LicenseCacheItem> GetLicensesFromApi()
        {
            return apiClient.GetLicenses()
                .WhereNotNull()
                .Select(LicenseCacheItem.Create);
        }

        IObservable<GitIgnoreCacheItem> GetGitIgnoreTemplatesFromApi()
        {
            return apiClient.GetGitIgnoreTemplates()
                .WhereNotNull()
                .Select(GitIgnoreCacheItem.Create);
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUser()
        {
            return hostCache.GetAndRefreshObject("user",
                () => apiClient.GetUser().Select(AccountCacheItem.Create), TimeSpan.FromMinutes(5), TimeSpan.FromDays(7))
                .TakeLast(1)
                .ToList();
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUserOrganizations()
        {
            return GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(user.Login + "|orgs",
                    () => apiClient.GetOrganizations().Select(AccountCacheItem.Create).ToList(),
                    TimeSpan.FromMinutes(2), TimeSpan.FromDays(7)))
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
                .TakeLast(1)
                .Concat(GetUserRepositories(RepositoryType.Member).TakeLast(1))
                .Concat(GetAllRepositoriesForAllOrganizations());
        }

        public IObservable<AccountCacheItem> GetUserFromCache()
        {
            return Observable.Defer(() => hostCache.GetObject<AccountCacheItem>("user"));
        }

        /// <summary>
        /// Gets a collection of Pull Requests. If you want to refresh existing data, pass a collection in
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public ITrackingCollection<IPullRequestModel> GetPullRequests(ISimpleRepositoryModel repo,
            ITrackingCollection<IPullRequestModel> collection)
        {
            // Since the api to list pull requests returns all the data for each pr, cache each pr in its own entry
            // and also cache an index that contains all the keys for each pr. This way we can fetch prs in bulk
            // but also individually without duplicating information. We store things in a custom observable collection
            // that checks whether an item is being updated (coming from the live stream after being retrieved from cache)
            // and replaces it instead of appending, so items get refreshed in-place as they come in.

            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}:{2}", CacheIndex.PRPrefix, user.Login, repo.Name));

            var source = Observable.Defer(() => keyobs
                .SelectMany(key =>
                    hostCache.GetAndFetchLatestFromIndex(key, () =>
                        apiClient.GetPullRequestsForRepository(repo.CloneUrl.Owner, repo.CloneUrl.RepositoryName)
                                 .Select(PullRequestCacheItem.Create),
                        item =>
                        {
                            // this could blow up due to the collection being disposed somewhere else
                            try { collection.RemoveItem(Create(item)); }
                            catch (ObjectDisposedException) { }
                        },
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromDays(1))
                )
                .Select(Create)
            );

            collection.Listen(source);
            return collection;
        }

        public ITrackingCollection<IRepositoryModel> GetRepositories(ITrackingCollection<IRepositoryModel> collection)
        {
            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}", CacheIndex.RepoPrefix, user.Login));

            var source = Observable.Defer(() => keyobs
                .SelectMany(key =>
                    hostCache.GetAndFetchLatestFromIndex(key, () =>
                        apiClient.GetRepositories()
                                 .Select(RepositoryCacheItem.Create),
                        item =>
                        {
                            // this could blow up due to the collection being disposed somewhere else
                            try { collection.RemoveItem(Create(item)); }
                            catch (ObjectDisposedException) { }
                        },
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromDays(1))
                )
                .Select(Create)
            );

            collection.Listen(source);
            return collection;
        }

        public IObservable<IPullRequestModel> CreatePullRequest(ISimpleRepositoryModel repository, string title, string body, IBranch source, IBranch target)
        {
            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}:{2}", CacheIndex.PRPrefix, user.Login, repository.Name));

            return Observable.Defer(() => keyobs
                .SelectMany(key =>
                    hostCache.PutAndUpdateIndex(key, () =>
                        apiClient.CreatePullRequest(new NewPullRequest(title, source.Name, target.Name) { Body = body },
                                    repository.CloneUrl.Owner,
                                    repository.CloneUrl.RepositoryName)
                                 .Select(PullRequestCacheItem.Create),
                    TimeSpan.FromMinutes(30))
                )
                .Select(Create)
            );
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
                        TimeSpan.FromMinutes(2),
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
                .SelectMany(org => GetOrganizationRepositories(org.Login).TakeLast(1));
        }

        IObservable<IReadOnlyList<IRepositoryModel>> GetOrganizationRepositories(string organization)
        {
            return Observable.Defer(() => GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(string.Format(CultureInfo.InvariantCulture, "{0}|{1}|repos", user.Login, organization),
                    () => apiClient.GetRepositoriesForOrganization(organization).Select(
                        RepositoryCacheItem.Create).ToList(),
                        TimeSpan.FromMinutes(2),
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

        public IObservable<IBranch> GetBranches(ISimpleRepositoryModel repo)
        {
            var keyobs = GetUserFromCache()
                .Select(user => string.Format(CultureInfo.InvariantCulture, "{0}|{1}|branch", user.Login, repo.Name));

            return Observable.Defer(() => keyobs
                    .SelectMany(key => apiClient.GetBranches(repo.CloneUrl.Owner, repo.CloneUrl.RepositoryName)))
                .Select(Create);
        }

        static GitIgnoreItem Create(GitIgnoreCacheItem item)
        {
            return GitIgnoreItem.Create(item.Name);
        }

        static LicenseItem Create(LicenseCacheItem licenseCacheItem)
        {
            return new LicenseItem(licenseCacheItem.Key, licenseCacheItem.Name);
        }

        IAccount Create(AccountCacheItem accountCacheItem)
        {
            return new Models.Account(
                accountCacheItem.Login,
                accountCacheItem.IsUser,
                accountCacheItem.IsEnterprise,
                accountCacheItem.OwnedPrivateRepositoriesCount,
                accountCacheItem.PrivateRepositoriesInPlanCount,
                avatarProvider.GetAvatar(accountCacheItem));
        }

        IRepositoryModel Create(RepositoryCacheItem item)
        {
            return new RepositoryModel(
                item.Id,
                item.Name,
                new UriString(item.CloneUrl),
                item.Private,
                item.Fork,
                Create(item.Owner))
            {
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        IPullRequestModel Create(PullRequestCacheItem prCacheItem)
        {
            return new PullRequestModel(
                prCacheItem.Number,
                prCacheItem.Title,
                Create(prCacheItem.Author),
                prCacheItem.Assignee != null ? Create(prCacheItem.Assignee) : null,
                prCacheItem.CreatedAt,
                prCacheItem.UpdatedAt)
            {
                CommentCount = prCacheItem.CommentCount,
                IsOpen = prCacheItem.IsOpen
            };
        }

        IBranch Create(Branch branch)
        {
            return new BranchModel(branch);
        }


        public IObservable<Unit> InsertUser(AccountCacheItem user)
        {
            return hostCache.InsertObject("user", user);
        }

        protected virtual void Dispose(bool disposing)
        {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public class GitIgnoreCacheItem : CacheItem
        {
            public static GitIgnoreCacheItem Create(string ignore)
            {
                return new GitIgnoreCacheItem { Key = ignore, Name = ignore, Timestamp = DateTime.Now };
            }

            public string Name { get; set; }
        }


        public class LicenseCacheItem : CacheItem
        {
            public static LicenseCacheItem Create(LicenseMetadata licenseMetadata)
            {
                return new LicenseCacheItem { Key = licenseMetadata.Key, Name = licenseMetadata.Name, Timestamp = DateTime.Now };
            }

            public string Name { get; set; }
        }

        public class RepositoryCacheItem : CacheItem
        {
            public static RepositoryCacheItem Create(Repository apiRepository)
            {
                return new RepositoryCacheItem(apiRepository);
            }

            public RepositoryCacheItem() {}

            public RepositoryCacheItem(Repository apiRepository)
            {
                Id = apiRepository.Id;
                Name = apiRepository.Name;
                Owner = AccountCacheItem.Create(apiRepository.Owner);
                CloneUrl = apiRepository.CloneUrl;
                Private = apiRepository.Private;
                Fork = apiRepository.Fork;
                Key = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Owner.Login, Name);
                CreatedAt = apiRepository.CreatedAt;
                UpdatedAt = apiRepository.UpdatedAt;
                Timestamp = apiRepository.UpdatedAt;
            }

            public long Id { get; set; }

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
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
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
                CommentCount = pr.Comments + pr.ReviewComments;
                Author = new AccountCacheItem(pr.User);
                Assignee = pr.Assignee != null ? new AccountCacheItem(pr.Assignee) : null;
                CreatedAt = pr.CreatedAt;
                UpdatedAt = pr.UpdatedAt;
                IsOpen = pr.State == ItemState.Open;
                Key = Number.ToString(CultureInfo.InvariantCulture);
                Timestamp = UpdatedAt;
            }

            [AllowNull]
            public string Title {[return: AllowNull] get; set; }
            public int Number { get; set; }
            public int CommentCount { get; set; }
            [AllowNull]
            public AccountCacheItem Author {[return: AllowNull] get; set; }
            [AllowNull]
            public AccountCacheItem Assignee { [return: AllowNull] get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
            public bool IsOpen { get; set; }
        }
    }
}
