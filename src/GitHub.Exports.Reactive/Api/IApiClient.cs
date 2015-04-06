using System;
using System.Collections.Generic;
using GitHub.Authentication;
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
        /// Retrieves all repositories that belong to this user or to any organizations this user is a member of.
        /// </summary>
        /// <returns></returns>
        IObservable<IEnumerable<Repository>> GetUserRepositories();
        IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(
            Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>> twoFactorChallengeHander = null,
            bool useOldScopes = false);
        IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(
            string authenticationCode,
            bool useOldScopes = false);
        ITwoFactorChallengeHandler TwoFactorChallengeHandler { get; }
        IObservable<string> GetGitIgnoreTemplates();
        IObservable<LicenseMetadata> GetLicenses();
    }
}
