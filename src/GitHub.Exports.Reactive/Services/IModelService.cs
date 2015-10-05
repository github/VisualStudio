using System;
using System.Collections.Generic;
using System.Reactive;
using GitHub.Caches;
using GitHub.Collections;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Class used to retrieve GitHub API data and turn them into models suitable for binding to in the UI.
    /// This handles the API retrieval and caching.
    /// </summary>
    public interface IModelService : IDisposable
    {
        IObservable<AccountCacheItem> GetUserFromCache();
        IObservable<Unit> InsertUser(AccountCacheItem user);
        IObservable<IReadOnlyList<IAccount>> GetAccounts();
        IObservable<IReadOnlyList<IRepositoryModel>> GetRepositories();
        IObservable<IReadOnlyList<LicenseItem>> GetLicenses();
        IObservable<IReadOnlyList<GitIgnoreItem>> GetGitIgnoreTemplates();
        IObservable<Unit> InvalidateAll();
        ITrackingCollection<IPullRequestModel> GetPullRequests(ISimpleRepositoryModel repo);
    }
}
