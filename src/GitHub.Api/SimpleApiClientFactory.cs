using GitHub.Models;
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
    public class SimpleApiClientFactory : ISimpleApiClientFactory
    {
        readonly ProductHeaderValue productHeader;

        [ImportingConstructor]
        public SimpleApiClientFactory(IProgram program)
        {
            productHeader = program.ProductHeader;
        }

        public ISimpleApiClient Create(Uri repoUrl)
        {
            var hostAddress = HostAddress.Create(repoUrl);
            var apiBaseUri = hostAddress.ApiUri;
            return new SimpleApiClient(hostAddress, repoUrl, new GitHubClient(productHeader, apiBaseUri));
        }
    }
}
