using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using NullGuard;
using Octokit;

namespace GitHub.Services
{
    [Export(typeof(IModelService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ModelService : IModelService
    {
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
                hostCache.GetAndFetchLatest("gitignores", GetOrderedGitIgnoreTemplatesFromApi)
                .ToReadOnlyList(GitIgnoreItem.Create, GitIgnoreItem.None));
        }

        public IObservable<IReadOnlyList<LicenseItem>> GetLicenses()
        {
            return Observable.Defer(() =>
                hostCache.GetAndFetchLatest("licenses", GetOrderedLicensesFromApi)
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
            return hostCache.GetAndFetchLatest("user", () => apiClient.GetUser().Select(AccountCacheItem.Create))
                .ToList();
        }

        IObservable<IEnumerable<AccountCacheItem>> GetUserOrganizations()
        {
            return Observable.Defer(() =>
                hostCache.GetAndFetchLatest("orgs",
                    () => apiClient.GetOrganizations().Select(AccountCacheItem.Create).ToList()));
        }

        public IObservable<IReadOnlyList<IRepositoryModel>> GetRepositories()
        {
            return Observable.Defer(() =>
                hostCache.GetAndFetchLatest("repos",
                    () => apiClient.GetUserRepositories().Select(RepositoryCacheItem.Create).ToList())
                .ToReadOnlyList(Create));
        }

        public IObservable<AccountCacheItem> GetUserFromCache()
        {
            return Observable.Defer(() => hostCache.GetObject<AccountCacheItem>("user"));
        }

        public IObservable<Unit> InvalidateAll()
        {
            return hostCache.InvalidateAll();
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
