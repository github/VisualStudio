using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Infrastructure;
using GitHub.Models;
using GitHub.Primitives;
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
        public ApiClientFactory(IKeychain keychain, IProgram program, ILoggingConfiguration config)
        {
            Keychain = keychain;
            productHeader = program.ProductHeader;
            config.Configure();
        }

        public Task<IGitHubClient> CreateGitHubClient(HostAddress hostAddress)
        {
            var credentialStore = new KeychainCredentialStore(Keychain, hostAddress);
            var result = new GitHubClient(productHeader, credentialStore, hostAddress.ApiUri);
            return Task.FromResult<IGitHubClient>(result);
        }

        public async Task<IApiClient> Create(HostAddress hostAddress)
        {
            return new ApiClient(
                hostAddress,
                new ObservableGitHubClient(await CreateGitHubClient(hostAddress)));
        }

        protected IKeychain Keychain { get; private set; }
    }
}
