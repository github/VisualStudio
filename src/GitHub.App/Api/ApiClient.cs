using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using GitHub.Primitives;
using NLog;
using NullGuard;
using Octokit;
using Octokit.Reactive;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using Octokit.Internal;
using System.Collections.Generic;

namespace GitHub.Api
{
    public partial class ApiClient : IApiClient
    {
        const string scopesHeader = "X-OAuth-Scopes";
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        const string ProductName = Info.ApplicationInfo.ApplicationDescription;

        readonly IObservableGitHubClient gitHubClient;
        // There are two sets of authorization scopes, old and new:
        // The old scopes must be used by older versions of Enterprise that don't support the new scopes:
        readonly string[] oldAuthorizationScopes = { "user", "repo", "gist" };
        // These new scopes include write:public_key, which allows us to add public SSH keys to an account:
        readonly string[] newAuthorizationScopes = { "user", "repo", "gist", "write:public_key" };
        readonly static Lazy<string> lazyNote = new Lazy<string>(() => ProductName + " on " + GetMachineNameSafe());
        readonly static Lazy<string> lazyFingerprint = new Lazy<string>(GetFingerprint);

        string ClientId { get; set; }
        string ClientSecret { get; set; }

        public ApiClient(HostAddress hostAddress, IObservableGitHubClient gitHubClient)
        {
            Configure();
            HostAddress = hostAddress;
            this.gitHubClient = gitHubClient;
        }

        partial void Configure();

        public IObservable<Repository> CreateRepository(NewRepository repository, string login, bool isUser)
        {
            Guard.ArgumentNotEmptyString(login, nameof(login));

            var client = gitHubClient.Repository;

            return (isUser ? client.Create(repository) : client.Create(login, repository));
        }

        public IObservable<Gist> CreateGist(NewGist newGist)
        {
            return gitHubClient.Gist.Create(newGist);
        }

        public IObservable<User> GetUser()
        {
            return gitHubClient.User.Current();
        }

        public IObservable<string[]> GetScopes()
        {
            return GetScopesInternal().ToObservable();
        }

        async Task<string[]> GetScopesInternal()
        {
            // If auth type is Basic we might be able to read /api/authorizations to get the 
            // current scopes. However this request sometimes gets mysteriously converted to an
            // OAuth request so if that doesn't work try reading from / and checking for the 
            // X-OAuth-Scopes header.
            var connection = gitHubClient.Connection;

            if (connection.Credentials.AuthenticationType == AuthenticationType.Basic)
            {
                try
                {
                    var response = await gitHubClient.Connection.Get<string>(
                        ApiUrls.Authorizations(),
                        TimeSpan.FromSeconds(3));

                    var json = new SimpleJsonSerializer();
                    var authorizations = json.Deserialize<Octokit.Authorization[]>(response.Body);
                    var scopes = new List<string>();

                    foreach (var authorization in authorizations)
                    {
                        scopes.AddRange(authorization.Scopes);
                    }

                    return scopes.Distinct().ToArray();
                }
                catch { }
            }

            try
            {
                var response = await gitHubClient.Connection.Get<string>(
                        new Uri("/", UriKind.Relative),
                        TimeSpan.FromSeconds(3));

                if (response.HttpResponse.Headers.ContainsKey(scopesHeader))
                {
                    return response.HttpResponse.Headers[scopesHeader]
                        .Split(',')
                        .Select(x => x.Trim())
                        .ToArray();
                }
            }
            catch { }

            return new string[0];
        }

        public IObservable<ApplicationAuthorization> GetOrCreateApplicationAuthenticationCode(
            Func<TwoFactorAuthorizationException, IObservable<TwoFactorChallengeResult>> twoFactorChallengeHander,
            string authenticationCode = null,
            bool useOldScopes = false,
            bool useFingerPrint = true)
        {
            var newAuthorization = new NewAuthorization
            {
                Scopes = useOldScopes
                    ? oldAuthorizationScopes
                    : newAuthorizationScopes,
                Note = lazyNote.Value,
                Fingerprint = useFingerPrint ? lazyFingerprint.Value : null
            };

            Func<TwoFactorAuthorizationException, IObservable<TwoFactorChallengeResult>> dispatchedHandler =
                ex => Observable.Start(() => twoFactorChallengeHander(ex), RxApp.MainThreadScheduler).Merge();

            var authorizationsClient = gitHubClient.Authorization;

            return string.IsNullOrEmpty(authenticationCode)
                ? authorizationsClient.CreateAndDeleteExistingApplicationAuthorization(
                        ClientId,
                        ClientSecret,
                        newAuthorization,
                        dispatchedHandler,
                        true)
                :   authorizationsClient.CreateAndDeleteExistingApplicationAuthorization(
                        ClientId,
                        ClientSecret,
                        newAuthorization,
                        dispatchedHandler,
                        authenticationCode,
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

        public HostAddress HostAddress { get; }

        static string GetSha256Hash(string input)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(input);
                    var hash = sha256.ComputeHash(bytes);

                    return string.Join("", hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
                }
            }
            catch (Exception e)
            {
                log.Error("IMPOSSIBLE! Generating Sha256 hash caused an exception.", e);
                return null;
            }
        }

        static string GetFingerprint()
        {
            return GetSha256Hash(ProductName + ":" + GetMachineIdentifier());
        }

        static string GetMachineNameSafe()
        {
            try
            {
                return Dns.GetHostName();
            }
            catch (Exception e)
            {
                log.Info("Failed to retrieve host name using `DNS.GetHostName`.", e);
                try
                {
                    return Environment.MachineName;
                }
                catch (Exception ex)
                {
                    log.Info("Failed to retrieve host name using `Environment.MachineName`.", ex);
                    return "(unknown)";
                }
            }
        }

        static string GetMachineIdentifier()
        {
            try
            {
                // adapted from http://stackoverflow.com/a/1561067
                var fastedValidNetworkInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .OrderBy(nic => nic.Speed)
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault(address => address.Length > 12);

                return fastedValidNetworkInterface ?? GetMachineNameSafe();
            }
            catch (Exception e)
            {
                log.Info("Could not retrieve MAC address. Fallback to using machine name.", e);
                return GetMachineNameSafe();
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

        public IObservable<PullRequest> GetPullRequestsForRepository(string owner, string name)
        {
            return gitHubClient.PullRequest.GetAllForRepository(owner, name,
                new PullRequestRequest {
                    State = ItemState.All,
                    SortProperty = PullRequestSort.Updated,
                    SortDirection = SortDirection.Descending
                });
        }
    }
}
