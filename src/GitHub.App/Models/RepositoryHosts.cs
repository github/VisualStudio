using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akavache;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Infrastructure;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
using Serilog;

namespace GitHub.Models
{
    [Export(typeof(IRepositoryHosts))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryHosts : ReactiveObject, IRepositoryHosts
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryHosts>();
        public static DisconnectedRepositoryHost DisconnectedRepositoryHost = new DisconnectedRepositoryHost();
        public const string EnterpriseHostApiBaseUriCacheKey = "enterprise-host-api-base-uri";
        readonly ObservableAsPropertyHelper<bool> isLoggedInToAnyHost;
        readonly IConnectionManager connectionManager;
        readonly CompositeDisposable disposables = new CompositeDisposable();

        [ImportingConstructor]
        public RepositoryHosts(
            IRepositoryHostFactory repositoryHostFactory,
            ISharedCache sharedCache,
            IConnectionManager connectionManager)
        {
            Guard.ArgumentNotNull(repositoryHostFactory, nameof(repositoryHostFactory));
            Guard.ArgumentNotNull(sharedCache, nameof(sharedCache));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));

            this.connectionManager = connectionManager;

            RepositoryHostFactory = repositoryHostFactory;
            GitHubHost = DisconnectedRepositoryHost;
            EnterpriseHost = DisconnectedRepositoryHost;

            var persistEntepriseHostObs = this.WhenAny(x => x.EnterpriseHost, x => x.Value)
                .Skip(1)  // The first value will be null or something already in the db
                .SelectMany(enterpriseHost =>
                {
                    if (!enterpriseHost.IsLoggedIn)
                    {
                        return sharedCache.UserAccount
                            .InvalidateObject<Uri>(EnterpriseHostApiBaseUriCacheKey)
                            .Catch<Unit, Exception>(ex =>
                            {
                                log.Warning(ex, "Failed to invalidate enterprise host uri");
                                return Observable.Return(Unit.Default);
                            });
                    }

                    return sharedCache.UserAccount
                        .InsertObject(EnterpriseHostApiBaseUriCacheKey, enterpriseHost.Address.ApiUri)
                        .Catch<Unit, Exception>(ex =>
                        {
                            log.Warning(ex, "Failed to persist enterprise host uri");
                            return Observable.Return(Unit.Default);
                        });
                });

            isLoggedInToAnyHost = this.WhenAny(
                x => x.GitHubHost.IsLoggedIn,
                x => x.EnterpriseHost.IsLoggedIn,
                (githubLoggedIn, enterpriseLoggedIn) => githubLoggedIn.Value || enterpriseLoggedIn.Value)
                .ToProperty(this, x => x.IsLoggedInToAnyHost);

            // This part is strictly to support having the IConnectionManager request that a connection
            // be logged in. It doesn't know about hosts or load anything reactive, so it gets
            // informed of logins by an observable returned by the event
            connectionManager.DoLogin += RunLoginHandler;

            // monitor the list of connections so we can log out hosts when connections are removed
            disposables.Add(
                connectionManager.Connections.CreateDerivedCollection(x => x)
                .ItemsRemoved
                .SelectMany(async x =>
                {
                    var host = LookupHost(x.HostAddress);
                    if (host.Address != x.HostAddress)
                        host = await RepositoryHostFactory.Create(x.HostAddress);
                    return host;
                })
                .Select(h => LogOut(h))
                .Merge().ToList().Select(_ => Unit.Default).Subscribe());


            // Wait until we've loaded (or failed to load) an enterprise uri from the db and then
            // start tracking changes to the EnterpriseHost property and persist every change to the db
            disposables.Add(persistEntepriseHostObs.Subscribe());
        }

        IObservable<IConnection> RunLoginHandler(IConnection connection)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));

            var handler = new ReplaySubject<IConnection>();
            var address = connection.HostAddress;
            var host = LookupHost(address);
            if (host == DisconnectedRepositoryHost)
                LogInFromCache(address)
                    .Subscribe(c => handler.OnNext(connection), () => handler.OnCompleted());
            else
            {
                handler.OnNext(connection);
                handler.OnCompleted();
            }
            return handler;
        }

        public IRepositoryHost LookupHost(HostAddress address)
        {
            if (address == GitHubHost.Address)
                return GitHubHost;
            if (address == EnterpriseHost.Address)
                return EnterpriseHost;
            return DisconnectedRepositoryHost;
        }

        public IObservable<AuthenticationResult> LogIn(
            HostAddress address,
            string usernameOrEmail,
            string password)
        {
            Guard.ArgumentNotNull(address, nameof(address));
            Guard.ArgumentNotEmptyString(usernameOrEmail, nameof(usernameOrEmail));
            Guard.ArgumentNotEmptyString(password, nameof(password));

            var isDotCom = HostAddress.GitHubDotComHostAddress == address;

            return Observable.Defer(async () =>
            {
                var host = await RepositoryHostFactory.Create(address);

                return host.LogIn(usernameOrEmail, password)
                    .Catch<AuthenticationResult, Exception>(Observable.Throw<AuthenticationResult>)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do(result =>
                    {
                        bool successful = result.IsSuccess();
                        log.Information("Log in to {host} host '{ApiUri}' with username '{usernameOrEmail}' {successful}",
                            isDotCom ? "GitHub.com" : address.WebUri.Host,
                            address.ApiUri,
                            usernameOrEmail,
                            successful ? "SUCCEEDED" : "FAILED"
                        );
                        if (successful)
                        {
                            // Make sure that GitHubHost/EnterpriseHost are set when the connections
                            // changed event is raised and likewise that the connection is added when
                            // the property changed notification is sent.
                            if (isDotCom)
                                githubHost = host;
                            else
                                enterpriseHost = host;

                            connectionManager.AddConnection(address, usernameOrEmail);

                            if (isDotCom)
                                this.RaisePropertyChanged(nameof(GitHubHost));
                            else
                                this.RaisePropertyChanged(nameof(EnterpriseHost));
                        }
                    });
            });
        }

        /// <summary>
        /// This is only called by the connection manager when logging in connections
        /// that already exist so we don't have to add the connection.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IObservable<AuthenticationResult> LogInFromCache(HostAddress address)
        {
            Guard.ArgumentNotNull(address, nameof(address));

            var isDotCom = HostAddress.GitHubDotComHostAddress == address;

            return Observable.Defer(async () =>
            {
                var host = await RepositoryHostFactory.Create(address);
                return host.LogInFromCache()
                    .Catch<AuthenticationResult, Exception>(Observable.Throw<AuthenticationResult>)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do(result =>
                    {
                        bool successful = result.IsSuccess();
                        if (successful)
                        {
                            if (isDotCom)
                                GitHubHost = host;
                            else
                                EnterpriseHost = host;
                        }
                    });
            });
        }

        public IObservable<Unit> LogOut(IRepositoryHost host)
        {
            Guard.ArgumentNotNull(host, nameof(host));

            var address = host.Address;
            var isDotCom = HostAddress.GitHubDotComHostAddress == address;
            return host.LogOut()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(result =>
                {
                    // reset the logged out host property to null
                    // it'll know what to do
                    if (isDotCom)
                        GitHubHost = null;
                    else
                        EnterpriseHost = null;
                    connectionManager.RemoveConnection(address);
                    RepositoryHostFactory.Remove(host);
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
                    connectionManager.DoLogin -= RunLoginHandler;
                    disposables.Dispose();
                }
                catch (Exception e)
                {
                    log.Warning(e, "Exception occured while disposing RepositoryHosts");
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
    }
}