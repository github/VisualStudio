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

        [ImportingConstructor]
        public SimpleApiClientFactory(IProgram program, Lazy<IEnterpriseProbe> enterpriseProbe)
        {
            productHeader = program.ProductHeader;
            lazyEnterpriseProbe = enterpriseProbe;
        }

        public ISimpleApiClient Create(Uri repoUrl)
        {
            var hostAddress = HostAddress.Create(repoUrl);
            var apiBaseUri = hostAddress.ApiUri;
            return new SimpleApiClient(hostAddress, repoUrl, new GitHubClient(productHeader, apiBaseUri), lazyEnterpriseProbe);
        }
    }
}
