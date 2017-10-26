using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using IGitHubClient = Octokit.IGitHubClient;
using GitHubClient = Octokit.GitHubClient;
using User = Octokit.User;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// Manages the configured <see cref="IConnection"/>s to GitHub instances.
    /// </summary>
    [Export(typeof(IConnectionManager))]
    public class ConnectionManager : IConnectionManager
    {
        readonly IProgram program;
        readonly IConnectionCache cache;
        readonly IKeychain keychain;
        readonly ILoginManager loginManager;
        readonly TaskCompletionSource<object> loaded;
        readonly Lazy<ObservableCollectionEx<IConnection>> connections;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public ConnectionManager(
            IProgram program,
            IConnectionCache cache,
            IKeychain keychain,
            ILoginManager loginManager,
            IUsageTracker usageTracker)
        {
            this.program = program;
            this.cache = cache;
            this.keychain = keychain;
            this.loginManager = loginManager;
            this.usageTracker = usageTracker;
            loaded = new TaskCompletionSource<object>();
            connections = new Lazy<ObservableCollectionEx<IConnection>>(
                this.CreateConnections,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc/>
        public IReadOnlyObservableCollection<IConnection> Connections => connections.Value;

        public Func<IConnection, Task> ConnectionCreated { get; set; }

        /// <inheritdoc/>
        public async Task<IConnection> GetConnection(HostAddress address)
        {
            return (await GetLoadedConnections()).FirstOrDefault(x => x.HostAddress == address);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyObservableCollection<IConnection>> GetLoadedConnections()
        {
            return await GetLoadedConnectionsInternal();
        }

        /// <inheritdoc/>
        public async Task<IConnection> LogIn(HostAddress address, string userName, string password)
        {
            var conns = await GetLoadedConnectionsInternal();

            if (conns.Any(x => x.HostAddress == address))
            {
                throw new InvalidOperationException($"A connection to {address} already exists.");
            }

            var client = CreateClient(address);
            var user = await loginManager.Login(address, client, userName, password);
            var connection = new Connection(address, userName, user, null);

            if (ConnectionCreated != null)
            {
                await ConnectionCreated(connection);
            }

            conns.Add(connection);
            await SaveConnections();
            await usageTracker.IncrementLoginCount();
            return connection;
        }

        /// <inheritdoc/>
        public async Task LogOut(HostAddress address)
        {
            var connection = await GetConnection(address);

            if (connection == null)
            {
                throw new KeyNotFoundException($"Could not find a connection to {address}.");
            }

            var client = CreateClient(address);
            await loginManager.Logout(address, client);
            connections.Value.Remove(connection);
            await SaveConnections();
        }

        ObservableCollectionEx<IConnection> CreateConnections()
        {
            var result = new ObservableCollectionEx<IConnection>();
            LoadConnections(result).Forget();
            return result;
        }

        IGitHubClient CreateClient(HostAddress address)
        {
            return new GitHubClient(
                program.ProductHeader,
                new KeychainCredentialStore(keychain, address),
                address.ApiUri);
        }

        async Task<ObservableCollectionEx<IConnection>> GetLoadedConnectionsInternal()
        {
            var result = Connections;
            await loaded.Task;
            return connections.Value;
        }

        async Task LoadConnections(ObservableCollection<IConnection> result)
        {
            try
            {
                foreach (var c in await cache.Load())
                {
                    var client = CreateClient(c.HostAddress);
                    User user = null;
                    Exception error = null;

                    try
                    {
                        user = await loginManager.LoginFromCache(c.HostAddress, client);
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }

                    var connection = new Connection(c.HostAddress, c.UserName, user, error);

                    if (ConnectionCreated != null)
                    {
                        await ConnectionCreated(connection);
                    }

                    result.Add(connection);
                    await usageTracker.IncrementLoginCount();
                }
            }
            finally
            {
                loaded.SetResult(null);
            }
        }

        async Task SaveConnections()
        {
            var conns = await GetLoadedConnectionsInternal();
            var details = conns.Select(x => new ConnectionDetails(x.HostAddress, x.Username));
            await cache.Save(details);
        }
    }
}
