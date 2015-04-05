using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Api;
using GitHub.Extensions.Reactive;
using Octokit;

namespace GitHub.Caches
{
    public class HostCache : IHostCache
    {
        readonly IBlobCache localMachineCache;
        readonly IBlobCache userAccountCache;
        readonly IApiClient apiClient;

        public HostCache(IBlobCache localMachineCache, IBlobCache userAccountCache, IApiClient apiClient)
        {
            this.localMachineCache = localMachineCache;
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
                localMachineCache.InvalidateAll(),
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
                localMachineCache.Dispose();
                localMachineCache.Shutdown.Wait();
                userAccountCache.Dispose();
                userAccountCache.Shutdown.Wait();
            }
        }
    }
}
