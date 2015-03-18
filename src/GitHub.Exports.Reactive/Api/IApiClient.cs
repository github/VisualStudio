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
        IObservable<SshKey> AddSshKey(SshKey newKey);
        IObservable<Repository> CreateRepository(Repository repo, string login, bool isUser);
        IObservable<SshKey> GetSshKeys();
        IObservable<User> GetUser();
        IObservable<User> GetAllUsersForAllOrganizations();
        IObservable<Organization> GetOrganization(string login);
        IObservable<Organization> GetOrganizations();
        IObservable<User> GetMembersOfOrganization(string organizationName);
        IObservable<Repository> GetRepository(string owner, string name);
        IObservable<IEnumerable<Repository>> GetUserRepositories(int currentUserId);
        IObservable<Repository> GetCurrentUserRepositoriesStreamed();
        IObservable<Repository> GetOrganizationRepositoriesStreamed(string login);
        IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(
            Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>> twoFactorChallengeHander = null,
            bool useOldScopes = false);
        IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(
            string authenticationCode,
            bool useOldScopes = false);
        IObservable<IReadOnlyList<EmailAddress>> GetEmails();
        ITwoFactorChallengeHandler TwoFactorChallengeHandler { get; }
    }
}
