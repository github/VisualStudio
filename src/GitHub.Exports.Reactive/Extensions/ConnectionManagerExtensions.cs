using System.Reactive.Linq;
using GitHub.Models;
using System;
using ReactiveUI;

namespace GitHub.Extensions
{
    public static class ConnectionManagerExtensions
    {
        public static IObservable<bool> IsLoggedIn(this IConnectionManager cm, IRepositoryHosts hosts)
        {
            return cm.Connections.ToObservable()
                    .SelectMany(c => c.Login())
                    .Select(c => hosts.LookupHost(c.HostAddress)).Any(h => h.IsLoggedIn);
        }
    }
}
