using System;
using System.ComponentModel.Composition;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;

namespace GitHub.Factories
{
    [Export(typeof(IRepositoryHostFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryHostFactory : IRepositoryHostFactory
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IHostCacheFactory hostCacheFactory;
        readonly ILoginCache loginCache;
        readonly IAvatarProvider avatarProvider;
        readonly ITwoFactorChallengeHandler twoFactorChallengeHandler;

        [ImportingConstructor]
        public RepositoryHostFactory(
            IApiClientFactory apiClientFactory,
            IHostCacheFactory hostCacheFactory,
            ILoginCache loginCache,
            IAvatarProvider avatarProvider,
            ITwoFactorChallengeHandler twoFactorChallengeHandler)
        {
            this.apiClientFactory = apiClientFactory;
            this.hostCacheFactory = hostCacheFactory;
            this.loginCache = loginCache;
            this.avatarProvider = avatarProvider;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;
        }

        public IRepositoryHost Create(HostAddress hostAddress)
        {
            var apiClient = apiClientFactory.Create(hostAddress);
            var hostCache = hostCacheFactory.Create(hostAddress);
            var modelService = new ModelService(apiClient, hostCache, avatarProvider);

            return new RepositoryHost(apiClient, modelService, loginCache, twoFactorChallengeHandler);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                loginCache.Dispose();
                avatarProvider.Dispose();
                hostCacheFactory.Dispose();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
