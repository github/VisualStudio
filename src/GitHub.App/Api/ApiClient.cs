using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Reactive.Linq;
using GitHub.Authentication;
using GitHub.Extensions;
using GitHub.Primitives;
using NLog;
using Octokit;
using Octokit.Reactive;
using ReactiveUI;
using Authorization = Octokit.Authorization;

namespace GitHub.Api
{
    public class ApiClient : IApiClient
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// https://github.com/
        /// </summary>
        public const string GitHubUrl = "https://" + GitHubDotComHostName;
        public const string GitHubDotComHostName = "github.com";
        public const string GitHubGistHostName = "gist.github.com";
        const string clientId = "fd5f729d309a7bfa8e1b";
        const string clientSecret = "ea0dc43463de55bb588d122cf9656109f850b6bd";
        public static readonly Uri GitHubDotComUri = new Uri(GitHubUrl);
        public static readonly Uri GitHubDotComApiBaseUri = new Uri("https://api." + GitHubDotComHostName);

        readonly IObservableGitHubClient gitHubClient;
        // There are two sets of authorization scopes, old and new:
        // The old scops must be used by older versions of Enterprise that don't support the new scopes:
        readonly string[] oldAuthorizationScopes = { "user", "repo" };
        // These new scopes include write:public_key, which allows us to add public SSH keys to an account:
        readonly string[] newAuthorizationScopes = { "user", "repo", "write:public_key" };

        public ApiClient(
            HostAddress hostAddress,
            IObservableGitHubClient gitHubClient,
            ITwoFactorChallengeHandler twoFactorChallengeHandler)
        {
            HostAddress = hostAddress;
            this.gitHubClient = gitHubClient;
            TwoFactorChallengeHandler = twoFactorChallengeHandler;
        }

        public HostAddress HostAddress { get; private set; }

        public IObservable<Repository> CreateRepository(Repository repo, string login, bool isUser)
        {
            Guard.ArgumentNotEmptyString(login, "login");

            var newRepository = ToNewRepository(repo);
            var client = gitHubClient.Repository;

            return (isUser ? client.Create(newRepository) : client.Create(login, newRepository));
        }

        public IObservable<SshKey> GetSshKeys()
        {
            return gitHubClient.SshKey.GetAllForCurrent();
        }

        public IObservable<SshKey> AddSshKey(SshKey newKey)
        {
            log.Info("About to add SSH Key: {0} - {1}", newKey.Title, newKey.Key);

            return gitHubClient.SshKey.Create(new SshKeyUpdate { Title = newKey.Title, Key = newKey.Key });
        }

        public IObservable<User> GetUser()
        {
            return gitHubClient.User.Current();
        }

        public IObservable<User> GetAllUsersForAllOrganizations()
        {
            return GetOrganizations().SelectMany(org => gitHubClient.Organization.Member.GetAll(org.Login));
        }

        public IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>> twoFactorChallengeHander = null, bool useOldScopes = false)
        {
            var newAuthorization = new NewAuthorization
            {
                Scopes = useOldScopes
                    ? oldAuthorizationScopes
                    : newAuthorizationScopes,
                Note = "GitHub for Visual Studio on " + GetMachineNameSafe()
            };

            var handler =  twoFactorChallengeHander ?? TwoFactorChallengeHandler.HandleTwoFactorException;

            Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>> dispatchedHandler =
                ex => Observable.Start(() => handler(ex), RxApp.MainThreadScheduler).SelectMany(result => result);

            return gitHubClient.Authorization.GetOrCreateApplicationAuthentication(
                clientId,
                clientSecret,
                newAuthorization,
                dispatchedHandler);
        }

        public IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(
            string authenticationCode,
            bool useOldScopes = false)
        {
            Guard.ArgumentNotEmptyString(authenticationCode, "authenticationCode");

            var newAuthorization = new NewAuthorization
            {
                Scopes = useOldScopes
                    ? oldAuthorizationScopes
                    : newAuthorizationScopes,
                Note = "GitHub for Visual Studio on " + GetMachineNameSafe()
            };

            return gitHubClient.Authorization.GetOrCreateApplicationAuthentication(
                clientId,
                clientSecret,
                newAuthorization,
                authenticationCode);
        }

        public IObservable<IReadOnlyList<EmailAddress>> GetEmails()
        {
            return gitHubClient.User.Email.GetAll()
                .ToArray()
                .Select(emails => new ReadOnlyCollection<EmailAddress>(emails));
        }

        public IObservable<Organization> GetOrganization(string login)
        {
            Guard.ArgumentNotEmptyString(login, "login");
            return gitHubClient.Organization.Get(login);
        }

        public IObservable<Organization> GetOrganizations()
        {
            return gitHubClient.Organization.GetAllForCurrent();
        }

        public IObservable<User> GetMembersOfOrganization(string organizationName)
        {
            return gitHubClient.Organization.Member.GetAll(organizationName);
        }

        IObservable<IEnumerable<Repository>> GetAllRepositoriesForOrganization(Organization org)
        {
            return gitHubClient.Repository.GetAllForOrg(org.Login)
                .ToList();
        }

        IObservable<IEnumerable<Repository>> GetAllRepositoriesForCurrentUser()
        {
            return gitHubClient.Repository.GetAllForCurrent()
                .ToList();
        }

        public IObservable<Repository> GetCurrentUserRepositoriesStreamed()
        {
            return gitHubClient.Repository.GetAllForCurrent();
        }

        public IObservable<Repository> GetOrganizationRepositoriesStreamed(string login)
        {
            return gitHubClient.Repository.GetAllForOrg(login);
        }

        public IObservable<Repository> GetRepository(string owner, string name)
        {
            return gitHubClient.Repository.Get(owner, name);
        }

        public IObservable<IEnumerable<Repository>> GetUserRepositories(int currentUserId)
        {
            return Observable.Merge(
                GetAllRepositoriesForCurrentUser(),
                GetOrganizations().SelectMany(GetAllRepositoriesForOrganization)
            );
        }

        static NewRepository ToNewRepository(Repository repository)
        {
            return new NewRepository
            {
                Name        = repository.Name,
                Description = repository.Description,
                Homepage    = repository.Homepage,
                Private     = repository.Private,
                HasIssues   = repository.HasIssues,
                HasWiki     = repository.HasWiki,
                HasDownloads= repository.HasDownloads
            };
        }

        static string GetMachineNameSafe()
        {
            try
            {
                return Dns.GetHostName();
            }
            catch (Exception)
            {
                try
                {
                    return Environment.MachineName;
                }
                catch (Exception)
                {
                    return "(unknown)";
                }
            }
        }

        public ITwoFactorChallengeHandler TwoFactorChallengeHandler { get; private set; }
    }
}
