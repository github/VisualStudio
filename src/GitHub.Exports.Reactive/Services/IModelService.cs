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
        IApiClient ApiClient { get; }

        IObservable<IAccount> GetCurrentUser();
        IObservable<IAccount> GetUser(string login);
        IObservable<Unit> InsertUser(AccountCacheItem user);
        IObservable<IReadOnlyList<IAccount>> GetAccounts();
        IObservable<RemoteRepositoryModel> GetRepository(string owner, string repo);
        IObservable<RemoteRepositoryModel> GetForks(RepositoryModel repository);
        IObservable<LicenseItem> GetLicenses();
        IObservable<GitIgnoreItem> GetGitIgnoreTemplates();
        IObservable<IPullRequestModel> GetPullRequest(string owner, string name, int number);
        IObservable<IPullRequestModel> CreatePullRequest(LocalRepositoryModel sourceRepository, RepositoryModel targetRepository,
            BranchModel sourceBranch, BranchModel targetBranch,
            string title, string body);
        IObservable<BranchModel> GetBranches(RepositoryModel repo);
        IObservable<Unit> InvalidateAll();
        IObservable<string> GetFileContents(RepositoryModel repo, string commitSha, string path, string fileSha);
    }
}
