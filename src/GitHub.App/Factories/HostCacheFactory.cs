using System;
using System.ComponentModel.Composition;
using System.IO;
using Akavache;
using GitHub.Primitives;
using Rothko;
using GitHub.Extensions;

namespace GitHub.Factories
{
    [Export(typeof(IHostCacheFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HostCacheFactory : IHostCacheFactory
    {
        readonly Lazy<IBlobCacheFactory> blobCacheFactory;
        readonly Lazy<IOperatingSystem> operatingSystem;
        
        [ImportingConstructor]
        public HostCacheFactory(Lazy<IBlobCacheFactory> blobCacheFactory, Lazy<IOperatingSystem> operatingSystem)
        {
            this.blobCacheFactory = blobCacheFactory;
            this.operatingSystem = operatingSystem;
        }

        public IBlobCache Create(HostAddress hostAddress)
        {
            var environment = OperatingSystem.Environment;
            // For GitHub.com, the cache file name should be "api.github.com.cache.db"
            // This is why we use ApiUrl and not CredentialCacheHostKey
            string host = hostAddress.ApiUri.Host;
            string cacheFileName = host + ".cache.db";

            var userAccountPath = Path.Combine(environment.GetApplicationDataPath(), cacheFileName);

            // CreateDirectory is a noop if the directory already exists.
            OperatingSystem.Directory.CreateDirectory(Path.GetDirectoryName(userAccountPath));
            return BlobCacheFactory.CreateBlobCache(userAccountPath);
        }

        IOperatingSystem OperatingSystem { get { return operatingSystem.Value; } }
        IBlobCacheFactory BlobCacheFactory { get { return blobCacheFactory.Value; } }

        protected virtual void Dispose(bool disposing)
        {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
