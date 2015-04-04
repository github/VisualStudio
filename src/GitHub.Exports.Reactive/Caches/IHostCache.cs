using System;
using System.Collections.Generic;
using System.Reactive;

namespace GitHub.Caches
{
    /// <summary>
    /// Per host cache data.
    /// </summary>
    public interface IHostCache : IDisposable
    {
        /// <summary>
        /// Retrieves user from the cache and then makes a request to update the cache.
        /// </summary>
        /// <returns></returns>
        IObservable<CachedAccount> GetAndFetchUser();
        IObservable<Unit> InsertUser(CachedAccount user);
        /// <summary>
        /// Retrieves organizations from the cache and then makes a request to update the cache.
        /// </summary>
        /// <returns></returns>
        IObservable<IEnumerable<CachedAccount>> GetAndFetchOrganizations();
        IObservable<Unit> InvalidateAll();
    }
}
