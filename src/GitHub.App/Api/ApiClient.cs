using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using GitHub.Authentication;
using GitHub.Primitives;
using NullGuard;
using Octokit;
using Octokit.Reactive;
using ReactiveUI;

namespace GitHub.Api
{
    public class ApiClient : IApiClient
    {
        const string ProductName = Info.ApplicationInfo.ApplicationDescription;
        const string clientId = "";
        const string clientSecret = "";

        readonly IObservableGitHubClient gitHubClient;
        // There are two sets of authorization scopes, old and new:
        // The old scopes must be used by older versions of Enterprise that don't support the new scopes:
        readonly string[] oldAuthorizationScopes = { "user", "repo" };
        // These new scopes include write:public_key, which allows us to add public SSH keys to an account:
        readonly string[] newAuthorizationScopes = { "user", "repo", "write:public_key" };
        static Lazy<string> lazyNote = new Lazy<string>(() => ProductName + " on " + GetMachineNameSafe());
        static Lazy<string> lazyFingerprint = new Lazy<string>(() => GetSha256Hash(lazyNote.Value));

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
            Func<TwoFactorAuthorizationException, IObservable<TwoFactorChallengeResult>> twoFactorChallengeHander,
            string authenticationCode = null,
            bool useOldScopes = false)
        {
            var newAuthorization = new NewAuthorization
            {
                Scopes = useOldScopes
                    ? oldAuthorizationScopes
                    : newAuthorizationScopes,
                Note = lazyNote.Value,
                Fingerprint = lazyFingerprint.Value
            };

            Func<TwoFactorAuthorizationException, IObservable<TwoFactorChallengeResult>> dispatchedHandler =
                ex => Observable.Start(() => twoFactorChallengeHander(ex), RxApp.MainThreadScheduler).Merge();

            var authorizationsClient = gitHubClient.Authorization;

            return authorizationsClient.CreateAndDeleteExistingApplicationAuthentication(
                        clientId,
                        clientSecret,
                        newAuthorization,
                        dispatchedHandler,
                        true);
        }

        public IObservable<Organization> GetOrganizations()
        {
            return gitHubClient.Organization.GetAllForCurrent();
        }

        public IObservable<Repository> GetUserRepositories(RepositoryType repositoryType)
        {
            var request = new RepositoryRequest
            {
                Type = repositoryType,
                Direction = SortDirection.Ascending,
                Sort = RepositorySort.FullName
            };
            return gitHubClient.Repository.GetAllForCurrent(request);
        }

        public IObservable<string> GetGitIgnoreTemplates()
        {
            return gitHubClient.Miscellaneous.GetAllGitIgnoreTemplates();
        }

        public IObservable<LicenseMetadata> GetLicenses()
        {
            return gitHubClient.Miscellaneous.GetAllLicenses();
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

        static string GetSha256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);

                return string.Join("", hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
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

        public IObservable<Repository> GetRepositoriesForOrganization(string organization)
        {
            return gitHubClient.Repository.GetAllForOrg(organization);
        }

        public IObservable<Unit> DeleteApplicationAuthorization(int id, [AllowNull]string twoFactorAuthorizationCode)
        {
            return gitHubClient.Authorization.Delete(id, twoFactorAuthorizationCode);
        }
    }
}
