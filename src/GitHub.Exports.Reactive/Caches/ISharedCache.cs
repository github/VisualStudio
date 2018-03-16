using System;
using System.Linq.Expressions;
using System.Reactive;
using Akavache;

namespace GitHub.Caches
{
    /// <summary>
    /// A cache for data that's not host specific
    /// </summary>
    public interface ISharedCache : IDisposable
    {
        IBlobCache UserAccount { get; }
        IBlobCache LocalMachine { get; }

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
    }
}
