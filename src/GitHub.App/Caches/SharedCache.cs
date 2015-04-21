using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Akavache;
using ReactiveUI;

namespace GitHub.Caches
{
    /// <summary>
    /// A cache for data that's not host specific
    /// </summary>
    [Export(typeof(ISharedCache))]
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UserAccount.Dispose();
                UserAccount.Shutdown.Wait();
                LocalMachine.Dispose();
                LocalMachine.Shutdown.Wait();
                Secure.Dispose();
                Secure.Shutdown.Wait();
            }
        }
    }
}
