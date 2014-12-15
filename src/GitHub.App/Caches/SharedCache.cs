using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Akavache;
using ReactiveUI;

namespace GitHub
{
    /// <summary>
    /// A cache for data that's not host specific
    /// </summary>
    [Export(typeof(ISharedCache))]
    public class SharedCache : ISharedCache
    {
        const string enterpriseHostApiBaseUriCacheKey = "enterprise-host-api-base-uri";
        const string staffModeKey = "__StaffOnlySettings__:IsStaffMode";

        // TODO Use this instead.
        //public SharedCache()
        //    : this(BlobCache.UserAccount, BlobCache.LocalMachine, BlobCache.Secure)
        //{
        //}

        public SharedCache() : this(new InMemoryBlobCache(), new InMemoryBlobCache(), new InMemoryBlobCache())
        {
        }

        protected SharedCache(IBlobCache userAccountCache, IBlobCache localMachineCache, ISecureBlobCache secureCache)
        {
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

        public IObservable<bool> GetStaffMode()
        {
            return UserAccount.GetOrFetchObject(staffModeKey, () => Observable.Return(false));
        }

        public IObservable<Unit> SetStaffMode(bool staffMode)
        {
            return UserAccount.InsertObject(staffModeKey, staffMode);
        }

        public IDisposable BindPropertyToCache<TSource, TProperty>(TSource source,
            Expression<Func<TSource, TProperty>> property,
            TProperty defaultValue)
        {
            return BindPropertyToCache(source, property, x => x, x => x, defaultValue);
        }

        public IDisposable BindPropertyToCache<TSource, TProperty, TCacheValue>(TSource source,
            Expression<Func<TSource, TProperty>> property,
            Func<TProperty, TCacheValue> mapToCache,
            Func<TCacheValue, TProperty> mapFromCache,
            TCacheValue defaultValue)
        {
            var propertyInfo = GetPropertyNameAndSetAction(property, source);
            var propertySetter = propertyInfo.Item2;
            propertySetter(mapFromCache(defaultValue));
            var restoreCachedValue = new Action<TCacheValue>(value => propertySetter(mapFromCache(value)));

            // TODO: We may want a way to preserve the cache key should we ever rename the property or type.
            //       We could look for specific attributes to do this. But for now, let's not worry about it.

            string cacheKey = "Application:" + typeof(TSource).Name + ":" + propertyInfo.Item1;
            var propertyObservable = source.WhenAny(property, prop => prop.Value).Select(mapToCache);

            return LocalMachine.GetObject<TCacheValue>(cacheKey)
                .Catch(Observable.Return(defaultValue))
                .SingleAsync()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(cachedValue =>
                {
                    restoreCachedValue(cachedValue);
                    propertyObservable
                        .Throttle(TimeSpan.FromSeconds(2), RxApp.TaskpoolScheduler)
                        .SelectMany(width => LocalMachine.InsertObject(cacheKey, width))
                        .Subscribe();
                });
        }

        static Tuple<string, Action<TProperty>> GetPropertyNameAndSetAction<TSource, TProperty>(
            Expression<Func<TSource, TProperty>> expression,
            TSource source)
        {
            var member = expression.Body as MemberExpression;
            Debug.Assert(member != null, "Expression should be a property and not method or some other shit.");
            var property = member.Member as PropertyInfo;
            Debug.Assert(property != null, "Expression should be a property and not field or some other shit.");
            var propertySetterAction = new Action<TProperty>(value => property.SetValue(source, value));
            return Tuple.Create(property.Name, propertySetterAction);
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
