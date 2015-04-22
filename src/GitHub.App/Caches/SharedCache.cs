using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;

namespace GitHub.Caches
{
    /// <summary>
    /// A cache for data that's not host specific
    /// </summary>
    [Export(typeof(ISharedCache))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SharedCache : ISharedCache
    {
        const string enterpriseHostApiBaseUriCacheKey = "enterprise-host-api-base-uri";

        static SharedCache()
        {
            BlobCache.ApplicationName = Info.ApplicationInfo.ApplicationName;
        }

        public SharedCache() : this(BlobCache.UserAccount, BlobCache.LocalMachine, null)
        {
        }

        protected SharedCache(IBlobCache userAccountCache, IBlobCache localMachineCache, ISecureBlobCache secureCache)
        {
            if (secureCache == null)
            {
                BlobCache.Secure = new CredentialCache();
                secureCache = BlobCache.Secure;
            }
            UserAccount = userAccountCache;
            LocalMachine = localMachineCache;
            Secure = secureCache;
        }

        public IBlobCache UserAccount { get; private set; }
        public IBlobCache LocalMachine { get; private set; }
        public ISecureBlobCache Secure { get; private set; }

        public IObservable<Uri> GetEnterpriseHostUri()
        {
            return UserAccount.GetObject<Uri>(enterpriseHostApiBaseUriCacheKey);
        }

        public IObservable<Unit> InsertEnterpriseHostUri(Uri enterpriseHostUri)
        {
            return UserAccount.InsertObject(enterpriseHostApiBaseUriCacheKey, enterpriseHostUri);
        }

        public IObservable<Unit> InvalidateEnterpriseHostUri()
        {
            return UserAccount.InvalidateObject<Uri>(enterpriseHostApiBaseUriCacheKey);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                UserAccount.Dispose();
                UserAccount.Shutdown.Wait();
                LocalMachine.Dispose();
                LocalMachine.Shutdown.Wait();
                Secure.Dispose();
                Secure.Shutdown.Wait();
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
