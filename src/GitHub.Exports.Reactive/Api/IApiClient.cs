using System;
using System.Reactive;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    public interface IApiClient
    {
        HostAddress HostAddress { get; }

        // HACK: This is temporary. Should be removed in login refactor timeframe.
        IGitHubClient GitHubClient { get; }

        IObservable<Repository> CreateRepository(NewRepository repository, string login, bool isUser);
        IObservable<Gist> CreateGist(NewGist newGist);
        IObservable<UserAndScopes> GetUser();
        IObservable<Organization> GetOrganizations();
        /// <summary>
        /// Retrieves all repositories that belong to this user.
        /// </summary>
        /// <returns></returns>
        IObservable<Repository> GetUserRepositories(RepositoryType repositoryType);
        /// <summary>
        /// Retrieves the repository for this org.
        /// </summary>
        /// <returns></returns>
        IObservable<Repository> GetRepositoriesForOrganization(string organization);
        IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(
            Func<TwoFactorAuthorizationException, IObservable<TwoFactorChallengeResult>> twoFactorChallengeHander,
            string authenticationCode = null,
            bool useOldScopes = false,
            bool useFingerprint = true);

        IObservable<string> GetGitIgnoreTemplates();
        IObservable<LicenseMetadata> GetLicenses();
        IObservable<Unit> DeleteApplicationAuthorization(int id, string twoFactorAuthorizationCode);
        IObservable<PullRequest> GetPullRequest(string owner, string name, int number);
        IObservable<PullRequestFile> GetPullRequestFiles(string owner, string name, int number);
        IObservable<PullRequest> GetPullRequestsForRepository(string owner, string name);
        IObservable<PullRequest> CreatePullRequest(NewPullRequest pullRequest, string owner, string repo);
        IObservable<Branch> GetBranches(string owner, string repo);
        IObservable<Repository> GetRepositories();
        IObservable<Repository> GetRepository(string owner, string repo);
        IObservable<RepositoryContent> GetFileContents(string owner, string name, string reference, string path);
    }
}
