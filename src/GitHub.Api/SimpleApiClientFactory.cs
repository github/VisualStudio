using System;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GitHub.Api
{
    [Export(typeof(ISimpleApiClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SimpleApiClientFactory : ISimpleApiClientFactory
    {
        readonly IKeychain keychain;
        readonly ProductHeaderValue productHeader;
        readonly Lazy<IEnterpriseProbeTask> lazyEnterpriseProbe;
        readonly Lazy<IWikiProbe> lazyWikiProbe;

        static readonly ConcurrentDictionary<UriString, ISimpleApiClient> cache = new ConcurrentDictionary<UriString, ISimpleApiClient>();

        [ImportingConstructor]
        public SimpleApiClientFactory(
            IProgram program,
            IKeychain keychain,
            Lazy<IEnterpriseProbeTask> enterpriseProbe,
            Lazy<IWikiProbe> wikiProbe)
        {
            this.keychain = keychain;
            productHeader = program.ProductHeader;
            lazyEnterpriseProbe = enterpriseProbe;
            lazyWikiProbe = wikiProbe;
        }

        public Task<ISimpleApiClient> Create(UriString repositoryUrl)
        {
            var hostAddress = HostAddress.Create(repositoryUrl);
            var credentialStore = new KeychainCredentialStore(keychain, hostAddress);
            var result = cache.GetOrAdd(repositoryUrl, new SimpleApiClient(repositoryUrl,
                new GitHubClient(productHeader, credentialStore, hostAddress.ApiUri),
                lazyEnterpriseProbe, lazyWikiProbe));
            return Task.FromResult(result);
        }

        public void ClearFromCache(ISimpleApiClient client)
        {
            ISimpleApiClient c;
            cache.TryRemove(client.OriginalUrl, out c);
        }
    }
}
