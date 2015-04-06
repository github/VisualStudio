using System.ComponentModel.Composition;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Factories
{
    [Export(typeof(IRepositoryHostFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryHostFactory : IRepositoryHostFactory
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IHostCacheFactory hostCacheFactory;
        readonly ILoginCache loginCache;
        readonly ITwoFactorChallengeHandler twoFactorChallengeHandler;

        [ImportingConstructor]
        public RepositoryHostFactory(
            IApiClientFactory apiClientFactory,
            IHostCacheFactory hostCacheFactory,
            ILoginCache loginCache,
            ITwoFactorChallengeHandler twoFactorChallengeHandler)
        {
            this.apiClientFactory = apiClientFactory;
            this.hostCacheFactory = hostCacheFactory;
            this.loginCache = loginCache;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;
        }

        public IRepositoryHost Create(HostAddress hostAddress)
        {
            var apiClient = apiClientFactory.Create(hostAddress);
            var hostCache = hostCacheFactory.Create(hostAddress);

            return new RepositoryHost(apiClient, hostCache, loginCache, twoFactorChallengeHandler);
        }
    }
}
