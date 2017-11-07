using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.Factories
{
    [Export(typeof(IRepositoryHostFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryHostFactory : IRepositoryHostFactory
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IHostCacheFactory hostCacheFactory;
        readonly IAvatarProvider avatarProvider;

        [ImportingConstructor]
        public RepositoryHostFactory(
            IApiClientFactory apiClientFactory,
            IHostCacheFactory hostCacheFactory,
            IAvatarProvider avatarProvider)
        {
            this.apiClientFactory = apiClientFactory;
            this.hostCacheFactory = hostCacheFactory;
            this.avatarProvider = avatarProvider;
        }

        public async Task<IRepositoryHost> Create(IConnection connection)
        {
            var hostAddress = connection.HostAddress;
            var apiClient = await apiClientFactory.Create(hostAddress);
            var hostCache = await hostCacheFactory.Create(hostAddress);
            var modelService = new ModelService(apiClient, hostCache, avatarProvider);
            var host = new RepositoryHost(connection, apiClient, modelService);
            return host;
        }
    }
}
