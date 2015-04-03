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
        IObservable<CachedAccount> GetUser();
        IObservable<Unit> InsertUser(CachedAccount user);
        IObservable<IEnumerable<CachedAccount>> GetAllOrganizations();
        IObservable<Unit> InvalidateAll();
    }
}
