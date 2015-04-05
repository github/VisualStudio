using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Api;
using GitHub.Extensions.Reactive;

namespace GitHub.Caches
{
    public class HostCache : IHostCache
    {
        readonly IBlobCache userAccountCache;
        readonly IApiClient apiClient;

        public HostCache(IBlobCache userAccountCache, IApiClient apiClient)
        {
            this.userAccountCache = userAccountCache;
            this.apiClient = apiClient;
        }

        public IObservable<CachedAccount> GetAndFetchUser()
        {
            return Observable.Defer(() => userAccountCache.GetAndFetchLatest("user",
                () => apiClient.GetUser().WhereNotNull().Select(user => new CachedAccount(user))));
        }

        public IObservable<Unit> InsertUser(CachedAccount user)
        {
            return userAccountCache.InsertObject("user", user);
        }

        public IObservable<IEnumerable<CachedAccount>> GetAndFetchOrganizations()
        {
            return Observable.Defer(() =>
                userAccountCache.GetAndFetchLatest("organizations",
                    () => apiClient.GetOrganizations().WhereNotNull().Select(org => new CachedAccount(org)).ToList()));
        }

        public IObservable<Unit> InvalidateAll()
        {
            return Observable.Merge
            (
                userAccountCache.InvalidateAll(),
                Invalidate()
            ).AsCompletion();
        }

        protected virtual IObservable<Unit> Invalidate()
        {
            return Observable.Return(Unit.Default);
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
                userAccountCache.Dispose();
                userAccountCache.Shutdown.Wait();
            }
        }
    }
}
