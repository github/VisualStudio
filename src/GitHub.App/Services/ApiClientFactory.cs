using System;
using System.ComponentModel.Composition;
using GitHub.Authentication;
using GitHub.Models;
using Octokit;
using Octokit.Reactive;

namespace GitHub.Api
{
    [Export(typeof(IApiClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ApiClientFactory : IApiClientFactory
    {
        readonly ITwoFactorChallengeHandler twoFactorChallengeHandler;
        readonly ProductHeaderValue productHeader;

        [ImportingConstructor]
        public ApiClientFactory(
            ILoginCache loginCache,
            ITwoFactorChallengeHandler twoFactorChallengeHandler,
            IProgram program)
        {
            LoginCache = loginCache;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;
            productHeader = program.ProductHeader;
        }

        public IApiClient Create(HostAddress hostAddress)
        {
            var apiBaseUri = hostAddress.ApiUri;

            return new ApiClient(
                hostAddress,
                new ObservableGitHubClient(
                    new GitHubClient(productHeader, new GitHubCredentialStore(hostAddress, LoginCache), apiBaseUri)),
                twoFactorChallengeHandler);
        }

        protected ILoginCache LoginCache { get; private set; }
    }
}
