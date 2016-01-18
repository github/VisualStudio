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

        static readonly Dictionary<Uri, ISimpleApiClient> cache = new Dictionary<Uri, ISimpleApiClient>();

        [ImportingConstructor]
        public SimpleApiClientFactory(IProgram program, Lazy<IEnterpriseProbeTask> enterpriseProbe, Lazy<IWikiProbe> wikiProbe)
        {
            productHeader = program.ProductHeader;
            lazyEnterpriseProbe = enterpriseProbe;
            lazyWikiProbe = wikiProbe;
        }

        public ISimpleApiClient Create(Uri repoUrl)
        {
            var contains = cache.ContainsKey(repoUrl);
            if (contains)
                return cache[repoUrl];

            lock (cache)
            {
                if (!cache.ContainsKey(repoUrl))
                {
                    var hostAddress = HostAddress.Create(repoUrl);
                    var apiBaseUri = hostAddress.ApiUri;
                    cache.Add(repoUrl, new SimpleApiClient(hostAddress, repoUrl, new GitHubClient(productHeader, new SimpleCredentialStore(hostAddress), apiBaseUri), lazyEnterpriseProbe, lazyWikiProbe));
                }
                return cache[repoUrl];
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
