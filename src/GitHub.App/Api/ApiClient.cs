using System;
using System.Net;
using System.Reactive.Linq;
using GitHub.Authentication;
using GitHub.Primitives;
using Octokit;
using Octokit.Reactive;
using ReactiveUI;

namespace GitHub.Api
{
    public class ApiClient : IApiClient
    {
        const string ProductName = Info.ApplicationInfo.ApplicationDescription;
        const string clientId = "fd5f729d309a7bfa8e1b";
        const string clientSecret = "ea0dc43463de55bb588d122cf9656109f850b6bd";

        readonly IObservableGitHubClient gitHubClient;
        // There are two sets of authorization scopes, old and new:
        // The old scopes must be used by older versions of Enterprise that don't support the new scopes:
        readonly string[] oldAuthorizationScopes = { "user", "repo" };
        // These new scopes include write:public_key, which allows us to add public SSH keys to an account:
        readonly string[] newAuthorizationScopes = { "user", "repo", "write:public_key" };

        public ApiClient(HostAddress hostAddress, IObservableGitHubClient gitHubClient)
        {
            HostAddress = hostAddress;
            this.gitHubClient = gitHubClient;
        }

        public IObservable<Repository> CreateRepository(NewRepository repository, string login, bool isUser)
        {
            Guard.ArgumentNotEmptyString(login, "login");

            var client = gitHubClient.Repository;

            return (isUser ? client.Create(repository) : client.Create(login, repository));
        }

        public IObservable<User> GetUser()
        {
            return gitHubClient.User.Current();
        }

        public IObservable<User> GetAllUsersForAllOrganizations()
        {
            return GetOrganizations().SelectMany(org => gitHubClient.Organization.Member.GetAll(org.Login));
        }

        public IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(
            Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>> twoFactorChallengeHander,
            bool useOldScopes = false)
        {
            var newAuthorization = new NewAuthorization
            {
                Scopes = useOldScopes
                    ? oldAuthorizationScopes
                    : newAuthorizationScopes,
                Note = ProductName + " on " + GetMachineNameSafe()
            };

            Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>> dispatchedHandler =
                ex => Observable.Start(() => twoFactorChallengeHander(ex), RxApp.MainThreadScheduler).Merge();

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
                Note = ProductName + " on " + GetMachineNameSafe()
            };

            return gitHubClient.Authorization.GetOrCreateApplicationAuthentication(
                clientId,
                clientSecret,
                newAuthorization,
                authenticationCode);
        }

        public IObservable<Organization> GetOrganizations()
        {
            return gitHubClient.Organization.GetAllForCurrent();
        }

        public IObservable<Repository> GetUserRepositories()
        {
            return Observable.Merge(
                GetAllRepositoriesForCurrentUser(),
                GetOrganizations().SelectMany(GetAllRepositoriesForOrganization)
            );
        }

        public IObservable<string> GetGitIgnoreTemplates()
        {
            return gitHubClient.Miscellaneous.GetGitIgnoreTemplates();
        }

        public IObservable<LicenseMetadata> GetLicenses()
        {
            return gitHubClient.Miscellaneous.GetLicenses();
        }

        public HostAddress HostAddress { get; private set; }

        public ITwoFactorChallengeHandler TwoFactorChallengeHandler { get; private set; }

        IObservable<Repository> GetAllRepositoriesForOrganization(Organization org)
        {
            return gitHubClient.Repository.GetAllForOrg(org.Login);
        }

        IObservable<Repository> GetAllRepositoriesForCurrentUser()
        {
            return gitHubClient.Repository.GetAllForCurrent();
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
    }
}
