using System.ComponentModel.Composition;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;
using Octokit.Reactive;
using ApiClient = GitHub.Api.ApiClient;
using GitHub.Infrastructure;
using System.Threading.Tasks;
using ILoginCache = GitHub.Caches.ILoginCache;

namespace GitHub.Factories
{
    [Export(typeof(IApiClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ApiClientFactory : IApiClientFactory
    {
        readonly ProductHeaderValue productHeader;

        [ImportingConstructor]
        public ApiClientFactory(ILoginCache loginCache, IProgram program, ILoggingConfiguration config)
        {
            LoginCache = loginCache;
            productHeader = program.ProductHeader;
            config.Configure();
        }

        public Task<IGitHubClient> CreateGitHubClient(HostAddress hostAddress)
        {
            return Task.FromResult<IGitHubClient>(new GitHubClient(
                productHeader,
                new GitHubCredentialStore(hostAddress, LoginCache),
                hostAddress.ApiUri));
        }

        public async Task<IApiClient> Create(HostAddress hostAddress)
        {
            return new ApiClient(
                hostAddress,
                new ObservableGitHubClient(await CreateGitHubClient(hostAddress)));
        }

        protected ILoginCache LoginCache { get; private set; }
    }
}
