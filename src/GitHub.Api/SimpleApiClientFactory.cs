using System;
using System.ComponentModel.Composition;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;

namespace GitHub.Api
{
    [Export(typeof(ISimpleApiClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SimpleApiClientFactory : ISimpleApiClientFactory
    {
        readonly ProductHeaderValue productHeader;
        readonly Lazy<IEnterpriseProbeTask> lazyEnterpriseProbe;
        readonly Lazy<IWikiProbe> lazyWikiProbe;

        [ImportingConstructor]
        public SimpleApiClientFactory(IProgram program, Lazy<IEnterpriseProbeTask> enterpriseProbe, Lazy<IWikiProbe> wikiProbe)
        {
            productHeader = program.ProductHeader;
            lazyEnterpriseProbe = enterpriseProbe;
            lazyWikiProbe = wikiProbe;
        }

        public ISimpleApiClient Create(Uri repoUrl)
        {
            var hostAddress = HostAddress.Create(repoUrl);
            var apiBaseUri = hostAddress.ApiUri;
            return new SimpleApiClient(hostAddress, repoUrl, new GitHubClient(productHeader, apiBaseUri), lazyEnterpriseProbe, lazyWikiProbe);
        }
    }
}
