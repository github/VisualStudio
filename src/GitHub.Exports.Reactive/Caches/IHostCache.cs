using System;
using System.Collections.Generic;
using System.Reactive;
using GitHub.Models;
using Octokit;

namespace GitHub.Caches
{
    /// <summary>
    /// Per host cache data.
    /// </summary>
    public interface IHostCache : IDisposable
    {
        IObservable<User> GetUser();
        IObservable<Unit> InsertUser(User user);
        IObservable<IEnumerable<Organization>> GetAllOrganizations();
        IObservable<Unit> InsertOrganization(Organization organization);
        IObservable<Unit> InvalidateOrganization(Organization organization);
        IObservable<Unit> InvalidateOrganization(IAccount organization);
        IObservable<Unit> InvalidateAll();
    }
}
