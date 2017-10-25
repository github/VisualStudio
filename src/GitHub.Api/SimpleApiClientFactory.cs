using System;
using System.Collections.Generic;
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
        readonly ProductHeaderValue productHeader;
        readonly Lazy<IEnterpriseProbeTask> lazyEnterpriseProbe;
        readonly Lazy<IWikiProbe> lazyWikiProbe;

        static readonly ConcurrentDictionary<UriString, ISimpleApiClient> cache = new ConcurrentDictionary<UriString, ISimpleApiClient>();

        [ImportingConstructor]
        public SimpleApiClientFactory(IProgram program, Lazy<IEnterpriseProbeTask> enterpriseProbe, Lazy<IWikiProbe> wikiProbe)
        {
            productHeader = program.ProductHeader;
            lazyEnterpriseProbe = enterpriseProbe;
            lazyWikiProbe = wikiProbe;
        }

        public async Task<ISimpleApiClient> Create(UriString repositoryUrl)
        {
            var hostAddress = HostAddress.Create(repositoryUrl);
            var result = cache.GetOrAdd(repositoryUrl, new SimpleApiClient(repositoryUrl,
                await CreateGitHubClient(hostAddress),
                lazyEnterpriseProbe, lazyWikiProbe));
            return result;
        }

        public Task<IGitHubClient> CreateGitHubClient(HostAddress address)
        {
            var result = new GitHubClient(productHeader, new SimpleCredentialStore(address), address.ApiUri);
            return Task.FromResult<IGitHubClient>(result);
        }

        public void ClearFromCache(ISimpleApiClient client)
        {
            ISimpleApiClient c;
            cache.TryRemove(client.OriginalUrl, out c);
        }
    }
}
