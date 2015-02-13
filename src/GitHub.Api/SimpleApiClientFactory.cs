using GitHub.Models;
using GitHub.Services;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Api
{
    [Export(typeof(ISimpleApiClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SimpleApiClientFactory : ISimpleApiClientFactory
    {
        readonly ProductHeaderValue productHeader;
        readonly Lazy<IEnterpriseProbe> lazyEnterpriseProbe;
        readonly Lazy<IWikiProbe> lazyWikiProbe;

        [ImportingConstructor]
        public SimpleApiClientFactory(IProgram program, Lazy<IEnterpriseProbe> enterpriseProbe, Lazy<IWikiProbe> wikiProbe)
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
