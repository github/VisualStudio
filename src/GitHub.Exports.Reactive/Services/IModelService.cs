using System;
using System.Collections.Generic;
using System.Reactive;
using GitHub.Models;
using GitHub.Caches;
using GitHub.Collections;
using GitHub.Api;

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
        IObservable<IRemoteRepositoryModel> GetRepository(string owner, string repo);
        ITrackingCollection<IRemoteRepositoryModel> GetRepositories(ITrackingCollection<IRemoteRepositoryModel> collection);
        IObservable<LicenseItem> GetLicenses();
        IObservable<GitIgnoreItem> GetGitIgnoreTemplates();
        IObservable<IPullRequestModel> GetPullRequest(string owner, string name, int number);
        ITrackingCollection<IPullRequestModel> GetPullRequests(IRepositoryModel repo, ITrackingCollection<IPullRequestModel> collection);
        IObservable<IPullRequestModel> CreatePullRequest(ILocalRepositoryModel sourceRepository, IRepositoryModel targetRepository,
            IBranch sourceBranch, IBranch targetBranch,
            string title, string body);
        IObservable<IBranch> GetBranches(IRepositoryModel repo);
        IObservable<Unit> InvalidateAll();
        IObservable<string> GetFileContents(IRepositoryModel repo, string commitSha, string path, string fileSha);
    }
}
