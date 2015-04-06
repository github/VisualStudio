using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;
using Octokit.Reactive;
using ApiClient = GitHub.Api.ApiClient;

namespace GitHub.Factories
{
    [Export(typeof(IApiClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ApiClientFactory : IApiClientFactory
    {
        readonly ProductHeaderValue productHeader;

        [ImportingConstructor]
        public ApiClientFactory(ILoginCache loginCache, IProgram program)
        {
            LoginCache = loginCache;
            productHeader = program.ProductHeader;
        }

        public IApiClient Create(HostAddress hostAddress)
        {
            var apiBaseUri = hostAddress.ApiUri;

            return new ApiClient(
                hostAddress,
                new ObservableGitHubClient(new GitHubClient(productHeader, new GitHubCredentialStore(hostAddress, LoginCache), apiBaseUri)));
        }

        protected ILoginCache LoginCache { get; private set; }
    }
}
