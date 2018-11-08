using System;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;

namespace GitHub.Extensions
{
    public static class ConnectionManagerExtensions
    {
        public static async Task<bool> IsLoggedIn(this IConnectionManager cm)
        {
            Guard.ArgumentNotNull(cm, nameof(cm));

            var connections = await cm.GetLoadedConnections();
            return connections.Any(x => x.ConnectionError == null);
        }

        public static async Task<bool> IsLoggedIn(this IConnectionManager cm, HostAddress address)
        {
            Guard.ArgumentNotNull(cm, nameof(cm));
            Guard.ArgumentNotNull(address, nameof(address));

            var connections = await cm.GetLoadedConnections();
            return connections.Any(x => x.HostAddress == address && x.ConnectionError == null);
        }

        public static async Task<IConnection> GetFirstLoggedInConnection(this IConnectionManager cm)
        {
            Guard.ArgumentNotNull(cm, nameof(cm));

            var connections = await cm.GetLoadedConnections();
            return connections.FirstOrDefault(x => x.IsLoggedIn);
        }

        public static Task<IConnection> GetConnection(this IConnectionManager cm, RepositoryModel repository)
        {
            if (repository?.CloneUrl != null)
            {
                var hostAddress = HostAddress.Create(repository.CloneUrl);
                return cm.GetConnection(hostAddress);
            }

            return null;
        }
    }
}
