using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Models;
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
                .ToReadOnlyList(GitIgnoreItem.Create, GitIgnoreItem.None));
        }

        public IObservable<IReadOnlyList<LicenseItem>> GetLicenses()
        {
            return Observable.Defer(() =>
                hostCache.GetAndRefreshObject(
                    "licenses",
                    GetOrderedLicensesFromApi,
                    TimeSpan.FromDays(1),
                    TimeSpan.FromDays(7))
                .ToReadOnlyList(Create, LicenseItem.None));
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
                .Select(licenses => licenses.OrderByDescending(lic => LicenseItem.IsRecommended(lic.Key)))
                .Catch<IEnumerable<LicenseCacheItem>, Exception>(_ => Observable.Return(Enumerable.Empty<LicenseCacheItem>()));
        }

        IObservable<IEnumerable<string>> GetOrderedGitIgnoreTemplatesFromApi()
        {
            return apiClient.GetGitIgnoreTemplates()
                .WhereNotNull()
                .ToList()
                .Select(templates => templates.OrderByDescending(GitIgnoreItem.IsRecommended))
                .Catch<IEnumerable<string>, Exception>(_ => Observable.Return(Enumerable.Empty<string>()));
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUser()
        {
            return hostCache.GetAndRefreshObject("user",
                () => apiClient.GetUser().Select(AccountCacheItem.Create), TimeSpan.FromMinutes(5), TimeSpan.FromDays(7))
                .ToList();
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUserOrganizations()
        {
            return GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(user.Login + "|orgs",
                    () => apiClient.GetOrganizations().Select(AccountCacheItem.Create).ToList(),
                    TimeSpan.FromMinutes(5), TimeSpan.FromDays(7)));
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

        static string GetSha256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);

                return string.Join("", hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        IObservable<IReadOnlyList<IRepositoryModel>> GetUserRepositories(RepositoryType repositoryType)
        {
            return Observable.Defer(() => GetUserFromCache().SelectMany(user =>
                hostCache.GetAndRefreshObject(string.Format(CultureInfo.InvariantCulture, "{0}|{1}:repos", user.Login, repositoryType),
                    () => GetUserRepositoriesFromApi(repositoryType),
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromDays(7)))
                .ToReadOnlyList(Create));
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
                .ToReadOnlyList(Create));
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
                repositoryCacheItem.CloneUrl,
                repositoryCacheItem.Private,
                repositoryCacheItem.Fork,
                Create(repositoryCacheItem.Owner));
        }

        public IObservable<Unit> InsertUser(AccountCacheItem user)
        {
            return hostCache.InsertObject("user", user);
        }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    try
                    {
                        hostCache.Dispose();
                    }
                    catch (Exception e)
                    {
                        log.Warn("Exception occured while disposing host cache", e);
                    }
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IObservable<string> GetOrGenerateFingerprint()
        {
            return hostCache.GetObject<string>("oauth-fingerprint")
                .Catch<string, KeyNotFoundException>(e => Observable.Return(Guid.NewGuid().ToString()));
        }

        public IObservable<Unit> SaveAuthorizationTokenId(int id)
        {
            return hostCache.InsertObject("oauth-token-id", id)
                .Catch<Unit, Exception>(e => Observable.Return(Unit.Default));
        }

        public IObservable<Unit> DeleteAuthorizationToken()
        {
            return hostCache.GetObject<int>("oauth-token-id")
                .SelectMany(apiClient.DeleteApplicationAuthorization)
                .Catch<Unit, Exception>(e =>
                {
                    log.Error("Could not delete authorization token", e);
                    return Observable.Return(Unit.Default);
                });
        }

        public class LicenseCacheItem
        {
            public static LicenseCacheItem Create(LicenseMetadata licenseMetadata)
            {
                return new LicenseCacheItem { Key = licenseMetadata.Key, Name = licenseMetadata.Name };
            }

            public LicenseCacheItem()
            {}

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
    }
}
