using System;
using System.ComponentModel.Composition;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using System.Reactive.Disposables;

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
        readonly CompositeDisposable hosts = new CompositeDisposable();
        readonly IUsageTracker usage;

        [ImportingConstructor]
        public RepositoryHostFactory(
            IApiClientFactory apiClientFactory,
            IHostCacheFactory hostCacheFactory,
            ILoginCache loginCache,
            IAvatarProvider avatarProvider,
            ITwoFactorChallengeHandler twoFactorChallengeHandler,
            IUsageTracker usage)
        {
            this.apiClientFactory = apiClientFactory;
            this.hostCacheFactory = hostCacheFactory;
            this.loginCache = loginCache;
            this.avatarProvider = avatarProvider;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;
            this.usage = usage;
        }

        public IRepositoryHost Create(HostAddress hostAddress)
        {
            var apiClient = apiClientFactory.Create(hostAddress);
            var hostCache = hostCacheFactory.Create(hostAddress);
            var modelService = new ModelService(apiClient, hostCache, avatarProvider);
            var host = new RepositoryHost(apiClient, modelService, loginCache, twoFactorChallengeHandler, usage);
            hosts.Add(host);
            return host;
        }

        public void Remove(IRepositoryHost host)
        {
            hosts.Remove(host);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;
                disposed = true;
                hosts.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
