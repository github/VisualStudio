using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Caches;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;

namespace GitHub.Factories
{
    [Export(typeof(IModelServiceFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class ModelServiceFactory : IModelServiceFactory, IDisposable
    {
        readonly IApiClientFactory apiClientFactory;
        readonly IGraphQLClientFactory graphQLClientFactory;
        readonly IHostCacheFactory hostCacheFactory;
        readonly IAvatarProvider avatarProvider;
        readonly Dictionary<IConnection, ModelService> cache = new Dictionary<IConnection, ModelService>();
        readonly SemaphoreSlim cacheLock = new SemaphoreSlim(1);

        [ImportingConstructor]
        public ModelServiceFactory(
            IApiClientFactory apiClientFactory,
            IGraphQLClientFactory graphQLClientFactory,
            IHostCacheFactory hostCacheFactory,
            IAvatarProvider avatarProvider)
        {
            this.apiClientFactory = apiClientFactory;
            this.graphQLClientFactory = graphQLClientFactory;
            this.hostCacheFactory = hostCacheFactory;
            this.avatarProvider = avatarProvider;
        }

        public async Task<IModelService> CreateAsync(IConnection connection)
        {
            ModelService result;

            await cacheLock.WaitAsync();

            try
            {
                if (!cache.TryGetValue(connection, out result))
                {
                    result = new ModelService(
                        await apiClientFactory.Create(connection.HostAddress),
                        await graphQLClientFactory.CreateConnection(connection.HostAddress),
                        await hostCacheFactory.Create(connection.HostAddress),
                        avatarProvider);
                    result.InsertUser(AccountCacheItem.Create(connection.User));
                    cache.Add(connection, result);
                }
            }
            finally
            {
                cacheLock.Release();
            }

            return result;
        }

        public IModelService CreateBlocking(IConnection connection)
        {
            return ThreadHelper.JoinableTaskFactory.Run(() => CreateAsync(connection));
        }

        public void Dispose() => cacheLock.Dispose();
    }
}
