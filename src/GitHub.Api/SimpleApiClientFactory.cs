using System;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;
using System.Collections.Generic;

namespace GitHub.Api
{
    [Export(typeof(ISimpleApiClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SimpleApiClientFactory : ISimpleApiClientFactory
    {
        readonly ProductHeaderValue productHeader;
        readonly Lazy<IEnterpriseProbeTask> lazyEnterpriseProbe;
        readonly Lazy<IWikiProbe> lazyWikiProbe;

        static readonly Dictionary<UriString, ISimpleApiClient> cache = new Dictionary<UriString, ISimpleApiClient>();

        [ImportingConstructor]
        public SimpleApiClientFactory(IProgram program, Lazy<IEnterpriseProbeTask> enterpriseProbe, Lazy<IWikiProbe> wikiProbe)
        {
            productHeader = program.ProductHeader;
            lazyEnterpriseProbe = enterpriseProbe;
            lazyWikiProbe = wikiProbe;
        }

        public ISimpleApiClient Create(UriString repositoryUrl)
        {
            var contains = cache.ContainsKey(repositoryUrl);
            if (contains)
                return cache[repositoryUrl];

            lock (cache)
            {
                if (!cache.ContainsKey(repositoryUrl))
                {
                    var hostAddress = HostAddress.Create(repositoryUrl);
                    var apiBaseUri = hostAddress.ApiUri;
                    cache.Add(repositoryUrl, new SimpleApiClient(hostAddress, repositoryUrl, new GitHubClient(productHeader, new SimpleCredentialStore(hostAddress), apiBaseUri), lazyEnterpriseProbe, lazyWikiProbe));
                }
                return cache[repositoryUrl];
            }
        }

        public void ClearFromCache(ISimpleApiClient client)
        {
            lock (cache)
            {
                if (cache.ContainsKey(client.OriginalUrl))
                    cache.Remove(client.OriginalUrl);
            }
        }
    }
}
