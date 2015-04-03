using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using GitHub.Caches;

namespace GitHub.Services
{
    [Export(typeof(IHostAvatarProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HostAvatarProvider : IHostAvatarProvider
    {
        readonly Lazy<ISharedCache> sharedCache;
        readonly Lazy<IImageCache> imageCache;

        [ImportingConstructor]
        public HostAvatarProvider(Lazy<ISharedCache> sharedCache, Lazy<IImageCache> imageCache)
        {
            this.sharedCache = sharedCache;
            this.imageCache = imageCache;
        }

        readonly ConcurrentDictionary<string, IAvatarProvider> providers = new ConcurrentDictionary<string, IAvatarProvider>();

        public IAvatarProvider Get(string gitHubBaseUrl)
        {
            return providers.GetOrAdd(gitHubBaseUrl, key =>
                new AvatarProvider(sharedCache.Value.LocalMachine, imageCache.Value, key));
        }
    }
}
