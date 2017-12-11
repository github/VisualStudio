using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Services
{
    [Export(typeof(IEnterpriseCapabilitiesService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class EnterpriseCapabilitiesService : IEnterpriseCapabilitiesService
    {
        static readonly Version MinimumOAuthVersion = new Version("2.12.1");
        readonly ISimpleApiClientFactory apiClientFactory;
        readonly IEnterpriseProbe probe;

        [ImportingConstructor]
        public EnterpriseCapabilitiesService(
            ISimpleApiClientFactory apiClientFactory,
            IEnterpriseProbe probe)
        {
            this.apiClientFactory = apiClientFactory;
            this.probe = probe;
        }

        public Task<EnterpriseProbeResult> Probe(Uri enterpriseBaseUrl) => probe.Probe(enterpriseBaseUrl);

        public async Task<EnterpriseLoginMethods> ProbeLoginMethods(Uri enterpriseBaseUrl)
        {
            var result = EnterpriseLoginMethods.Token;
            var client = await apiClientFactory.Create(UriString.ToUriString(enterpriseBaseUrl)).ConfigureAwait(false);
            var meta = await GetMetadata(client.Client).ConfigureAwait(false);

            if (meta.VerifiablePasswordAuthentication) result |= EnterpriseLoginMethods.UsernameAndPassword;

            if (meta.InstalledVersion != null)
            {
                var version = new Version(meta.InstalledVersion);
                if (version >= MinimumOAuthVersion) result |= EnterpriseLoginMethods.OAuth;
            }

            return result;
        }

        private Uri GetLoginUrl(IConnection connection)
        {
            var oauthClient = new OauthClient(connection);
            var oauthLoginRequest = new OauthLoginRequest(ApiClientConfiguration.ClientId)
            {
                RedirectUri = new Uri(OAuthCallbackListener.CallbackUrl),
            };
            var uri = oauthClient.GetGitHubLoginUrl(oauthLoginRequest);

            // OauthClient.GetGitHubLoginUrl seems to give the wrong URL. Fix this.
            return new Uri(uri.ToString().Replace("/api/v3", ""));
        }

        private async Task<EnterpriseMeta> GetMetadata(IGitHubClient client)
        {
            var endpoint = new Uri("meta", UriKind.Relative);
            var response = await client.Connection.Get<EnterpriseMeta>(endpoint, null, null).ConfigureAwait(false);
            return response.Body;
        }

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Created via Octokit reflection")]
        class EnterpriseMeta : Meta
        {
            public string InstalledVersion { get; private set; }
        }
    }
}
