using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;

namespace GitHub.VisualStudio
{
    [Export(typeof(IConnectionManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ConnectionManager : IConnectionManager
    {
        readonly IConnectionCache cache;
        readonly IKeychain keychain;
        readonly ILoginManager loginManager;
        readonly IApiClientFactory apiClientFactory;

        public event Func<IConnection, IObservable<IConnection>> DoLogin;

        [ImportingConstructor]
        public ConnectionManager(
            IConnectionCache cache,
            IKeychain keychain,
            ILoginManager loginManager,
            IApiClientFactory apiClientFactory)
        {
            this.cache = cache;
            this.keychain = keychain;
            this.loginManager = loginManager;
            this.apiClientFactory = apiClientFactory;

            Connections = new ObservableCollection<IConnection>();
            LoadConnectionsFromCache().Forget();
        }

        public IConnection CreateConnection(HostAddress address, string username)
        {
            return SetupConnection(address, username);
        }

        public bool AddConnection(HostAddress address, string username)
        {
            if (Connections.FirstOrDefault(x => x.HostAddress.Equals(address)) != null)
                return false;
            Connections.Add(SetupConnection(address, username));
            return true;
        }

        void AddConnection(Uri hostUrl, string username)
        {
            var address = HostAddress.Create(hostUrl);
            if (Connections.FirstOrDefault(x => x.HostAddress.Equals(address)) != null)
                return;
            var conn = SetupConnection(address, username);
            Connections.Add(conn);
        }

        public bool RemoveConnection(HostAddress address)
        {
            var c = Connections.FirstOrDefault(x => x.HostAddress.Equals(address));
            if (c == null)
                return false;
            RequestLogout(c);
            return true;
        }

        public IObservable<IConnection> RequestLogin(IConnection connection)
        {
            var handler = DoLogin;
            if (handler == null)
                return null;
            return handler(connection);
        }

        public void RequestLogout(IConnection connection)
        {
            Connections.Remove(connection);
        }

        IConnection SetupConnection(HostAddress address, string username)
        {
            var conn = new Connection(this, address, username);
            return conn;
        }

        void RefreshConnections(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (IConnection c in e.OldItems)
                {
                    // RepositoryHosts hasn't been loaded so it can't handle logging out, we have to do it ourselves
                    if (DoLogin == null)
                        keychain.Delete(c.HostAddress).Forget();
                }
            }

            SaveConnectionsToCache().Forget();
        }

        async Task LoadConnectionsFromCache()
        {
            foreach (var c in await cache.Load())
            {
                var client = await apiClientFactory.CreateGitHubClient(c.HostAddress);
                var addConnection = true;

                try
                {
                    await loginManager.LoginFromCache(c.HostAddress, client);
                }
                catch (Octokit.ApiException e)
                {
                    addConnection = false;
                    VsOutputLogger.WriteLine("Cached credentials for connection {0} were invalid: {1}", c.HostAddress, e);
                }
                catch (Exception)
                {
                    // Add the connection in this case - could be that there's no internet connection.
                }

                if (addConnection)
                {
                    AddConnection(c.HostAddress, c.UserName);
                }
            }

            Connections.CollectionChanged += RefreshConnections;
        }

        async Task SaveConnectionsToCache()
        {
            await cache.Save(Connections.Select(x => new ConnectionDetails(x.HostAddress, x.Username)));
        }

        public ObservableCollection<IConnection> Connections { get; private set; }
    }
}
