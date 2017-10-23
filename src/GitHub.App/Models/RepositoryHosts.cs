using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Authentication;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.Models
{
    [Export(typeof(IRepositoryHosts))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryHosts : ReactiveObject, IRepositoryHosts
    {
        static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static DisconnectedRepositoryHost DisconnectedRepositoryHost = new DisconnectedRepositoryHost();
        public const string EnterpriseHostApiBaseUriCacheKey = "enterprise-host-api-base-uri";
        readonly ObservableAsPropertyHelper<bool> isLoggedInToAnyHost;
        readonly IConnectionManager connectionManager;
        readonly CompositeDisposable disposables = new CompositeDisposable();
        readonly TaskCompletionSource<object> loaded;

        [ImportingConstructor]
        public RepositoryHosts(
            IRepositoryHostFactory repositoryHostFactory,
            IConnectionManager connectionManager)
        {
            Guard.ArgumentNotNull(repositoryHostFactory, nameof(repositoryHostFactory));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));

            this.connectionManager = connectionManager;

            RepositoryHostFactory = repositoryHostFactory;
            GitHubHost = DisconnectedRepositoryHost;
            EnterpriseHost = DisconnectedRepositoryHost;

            isLoggedInToAnyHost = this.WhenAny(
                x => x.GitHubHost.IsLoggedIn,
                x => x.EnterpriseHost.IsLoggedIn,
                (githubLoggedIn, enterpriseLoggedIn) => githubLoggedIn.Value || enterpriseLoggedIn.Value)
                .ToProperty(this, x => x.IsLoggedInToAnyHost);

            connectionManager.ConnectionCreated = ConnectionAdded;

            loaded = new TaskCompletionSource<object>();
            disposables.Add(connectionManager.Connections.ForEachItem(_ => { }, ConnectionRemoved, ConnectionsReset));
            Load();
        }

        public Task EnsureInitialized() => loaded.Task;

        public IRepositoryHost LookupHost(HostAddress address)
        {
            if (address == GitHubHost.Address)
                return GitHubHost;
            if (address == EnterpriseHost.Address)
                return EnterpriseHost;
            return DisconnectedRepositoryHost;
        }

        public IObservable<AuthenticationResult> LogIn(HostAddress address, string username, string password)
        {
            Guard.ArgumentNotNull(address, nameof(address));
            Guard.ArgumentNotEmptyString(username, nameof(username));
            Guard.ArgumentNotEmptyString(password, nameof(password));

            return Observable.Defer(async () =>
            {
                if (GitHubHost != DisconnectedRepositoryHost)
                {
                    await connectionManager.LogOut(address);
                }

                var connection = await connectionManager.LogIn(address, username, password);
                return Observable.Return(AuthenticationResult.Success);
            });
        }
        
        public IObservable<Unit> LogOut(IRepositoryHost host)
        {
            Guard.ArgumentNotNull(host, nameof(host));

            return Observable.Defer(async () =>
            {
                await connectionManager.LogOut(host.Address);
                return Observable.Return(Unit.Default);
            });
        }

        bool disposed;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                try
                {
                    disposables.Dispose();
                }
                catch (Exception e)
                {
                    log.Warn("Exception occured while disposing RepositoryHosts", e);
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        IRepositoryHost githubHost;
        public IRepositoryHost GitHubHost
        {
            get { return githubHost; }
            private set
            {
                var newHost = value ?? DisconnectedRepositoryHost;
                this.RaiseAndSetIfChanged(ref githubHost, newHost);
            }
        }

        IRepositoryHost enterpriseHost;
        public IRepositoryHost EnterpriseHost
        {
            get { return enterpriseHost; }
            set
            {
                var newHost = value ?? DisconnectedRepositoryHost;
                this.RaiseAndSetIfChanged(ref enterpriseHost, newHost);
            }
        }

        public IRepositoryHostFactory RepositoryHostFactory { get; private set; }

        public bool IsLoggedInToAnyHost { get { return isLoggedInToAnyHost.Value; } }

        async Task ConnectionAdded(IConnection connection)
        {
            try
            {
                var host = await RepositoryHostFactory.Create(connection);

                if (connection.HostAddress == HostAddress.GitHubDotComHostAddress)
                    GitHubHost = host;
                else
                    EnterpriseHost = host;
            }
            catch (Exception e)
            {
                log.Error("Error adding repository host.", e);
            }
        }

        void ConnectionRemoved(IConnection connection)
        {
            if (connection.HostAddress == HostAddress.GitHubDotComHostAddress)
            {
                GitHubHost = DisconnectedRepositoryHost;
            }
            else if (connection.HostAddress == EnterpriseHost.Address)
            {
                EnterpriseHost = DisconnectedRepositoryHost;
            }
        }

        void ConnectionsReset()
        {
            throw new NotSupportedException("ConnectionManager.Connections should never be reset.");
        }

        async void Load()
        {
            try
            {
                foreach (var connection in connectionManager.Connections)
                {
                    await ConnectionAdded(connection);
                }
            }
            catch (Exception e)
            {
                log.Error("Error loading repository hosts.", e);
            }
            finally
            {
                loaded.SetResult(null);
            }
        }
    }
}