using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Api;
using GitHub.Extensions.Reactive;
using GitHub.Models;
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

        public IObservable<User> GetUser()
        {
            return Observable.Defer(() => userAccountCache.GetAndFetchLatest("user", () => apiClient.GetUser()));
        }

        public IObservable<Unit> InsertUser(User user)
        {
            return userAccountCache.InsertObject("user", user);
        }

        public IObservable<IEnumerable<Organization>> GetAllOrganizations()
        {
            return Observable.Defer(() =>
                userAccountCache.GetAndFetchLatest("organizations", () => apiClient.GetOrganizations().ToList()));
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "We store the user differently")]
        public IObservable<Unit> InsertOrganization(Organization organization)
        {
            return userAccountCache.InsertObject(organization.Login, organization);
        }

        public IObservable<Unit> InvalidateOrganization(Organization organization)
        {
            return InvalidateOrganization(organization.Login);
        }

        public IObservable<Unit> InvalidateOrganization(IAccount organization)
        {
            return InvalidateOrganization(organization.Login);
        }

        IObservable<Unit> InvalidateOrganization(string login)
        {
            Guard.ArgumentNotEmptyString(login, "login");

            return userAccountCache.InvalidateObject<Organization>(login);
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
