using System;
using System.Reactive;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    public interface IApiClient
    {
        HostAddress HostAddress { get; }
        IObservable<Repository> CreateRepository(NewRepository repository, string login, bool isUser);
        IObservable<User> GetUser();
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
        IObservable<PullRequest> GetPullRequestsForRepository(string owner, string name);
    }
}
