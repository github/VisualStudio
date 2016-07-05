using System;
using System.Collections.Generic;
using System.Reactive;
using GitHub.Models;
using GitHub.Caches;
using GitHub.Collections;

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
        ITrackingCollection<IRepositoryModel> GetRepositories(ITrackingCollection<IRepositoryModel> collection);
        IObservable<LicenseItem> GetLicenses();
        IObservable<GitIgnoreItem> GetGitIgnoreTemplates();
        ITrackingCollection<IPullRequestModel> GetPullRequests(ISimpleRepositoryModel repo, ITrackingCollection<IPullRequestModel> collection);
        IObservable<IPullRequestModel> CreatePullRequest(ISimpleRepositoryModel repository, string title, IBranch source, IBranch target);
        IObservable<IBranch> GetBranches(ISimpleRepositoryModel repo);
        IObservable<Unit> InvalidateAll();
    }
}
