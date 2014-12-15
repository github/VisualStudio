using System;
using System.Linq.Expressions;
using System.Reactive;
using Akavache;

namespace GitHub
{
    /// <summary>
    /// A cache for data that's not host specific
    /// </summary>
    public interface ISharedCache : IDisposable
    {
        IBlobCache UserAccount { get; }
        IBlobCache LocalMachine { get; }
        ISecureBlobCache Secure { get; }

        /// <summary>
        /// Retrieves the Enterpise Host Uri from cache if present.
        /// </summary>
        /// <returns></returns>
        IObservable<Uri> GetEnterpriseHostUri();

        /// <summary>
        /// Inserts the Enterprise Host Uri.
        /// </summary>
        /// <returns></returns>
        IObservable<Unit> InsertEnterpriseHostUri(Uri enterpriseHostUri);

        /// <summary>
        /// Removes the Enterprise Host Uri from the cache.
        /// </summary>
        /// <returns></returns>
        IObservable<Unit> InvalidateEnterpriseHostUri();

        /// <summary>
        /// Retrieves whether staff mode is enabled.
        /// </summary>
        /// <returns></returns>
        IObservable<bool> GetStaffMode();

        /// <summary>
        /// Helper method used to bind a property of a view model to a value in the cache.
        /// </summary>
        IDisposable BindPropertyToCache<TSource, TProperty>(TSource source,
            Expression<Func<TSource, TProperty>> property,
            TProperty defaultValue);

        /// <summary>
        /// Helper method used to bind a property of a view model to a value in the cache with a mapping function to
        /// and from the cache.
        /// </summary>
        IDisposable BindPropertyToCache<TSource, TProperty, TCacheValue>(TSource source,
            Expression<Func<TSource, TProperty>> property,
            Func<TProperty, TCacheValue> mapToCache,
            Func<TCacheValue, TProperty> mapFromCache,
            TCacheValue defaultValue);

        /// <summary>
        /// Sets whether staff mode is enabled;
        /// </summary>
        /// <param name="staffMode"></param>
        /// <returns></returns>
        IObservable<Unit> SetStaffMode(bool staffMode);
    }
}
