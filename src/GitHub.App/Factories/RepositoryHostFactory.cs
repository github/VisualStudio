using System.ComponentModel.Composition;
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
        readonly IAccountFactory accountFactory;

        [ImportingConstructor]
        public RepositoryHostFactory(
            IApiClientFactory apiClientFactory,
            IHostCacheFactory hostCacheFactory,
            ILoginCache loginCache,
            IAccountFactory accountFactory)
        {
            this.apiClientFactory = apiClientFactory;
            this.hostCacheFactory = hostCacheFactory;
            this.loginCache = loginCache;
            this.accountFactory = accountFactory;
        }

        public IRepositoryHost Create(HostAddress hostAddress)
        {
            var apiClient = apiClientFactory.Create(hostAddress);
            var hostCache = hostCacheFactory.Create(hostAddress, apiClient);

            return new RepositoryHost(apiClient, hostCache, loginCache, accountFactory);
        }
    }
}
