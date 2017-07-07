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
        IObservable<IAccount> GetCurrentUser();
        IObservable<Unit> InsertUser(AccountCacheItem user);
        IObservable<IReadOnlyList<IAccount>> GetAccounts();
        ITrackingCollection<IRemoteRepositoryModel> GetRepositories(ITrackingCollection<IRemoteRepositoryModel> collection);
        IObservable<LicenseItem> GetLicenses();
        IObservable<GitIgnoreItem> GetGitIgnoreTemplates();
        IObservable<IPullRequestModel> GetPullRequest(ILocalRepositoryModel repo, int number);
        ITrackingCollection<IPullRequestModel> GetPullRequests(ILocalRepositoryModel repo, ITrackingCollection<IPullRequestModel> collection);
        IObservable<IPullRequestModel> CreatePullRequest(ILocalRepositoryModel sourceRepository, IRepositoryModel targetRepository,
            IBranch sourceBranch, IBranch targetBranch,
            string title, string body);
        IObservable<IBranch> GetBranches(IRepositoryModel repo);
        IObservable<Unit> InvalidateAll();
        IObservable<string> GetFileContents(IRepositoryModel repo, string commitSha, string path, string fileSha);
    }
}
