using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Primitives;
using ReactiveUI;
using System.Globalization;
using NullGuard;
using System.Linq;
using System.Reactive.Subjects;

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

        [ImportingConstructor]
        public RepositoryHosts(
            IRepositoryHostFactory repositoryHostFactory,
            ISharedCache sharedCache,
            IConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;

            RepositoryHostFactory = repositoryHostFactory;

            GitHubHost = DisconnectedRepositoryHost;
            EnterpriseHost = DisconnectedRepositoryHost;

            var initialCacheLoadObs = sharedCache.UserAccount.GetObject<Uri>(EnterpriseHostApiBaseUriCacheKey)
                .Catch<Uri, KeyNotFoundException>(_ => Observable.Return<Uri>(null))
                .Catch<Uri, Exception>(ex =>
                {
                    log.Warn("Failed to get Enterprise host URI from cache.", ex);
                    return Observable.Return<Uri>(null);
                })
                .WhereNotNull()
                .Select(HostAddress.Create)
                .Select(repositoryHostFactory.Create)
                .Do(x => EnterpriseHost = x)
                .SelectUnit();

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
                                log.Warn("Failed to invalidate enterprise host uri", ex);
                                return Observable.Return(Unit.Default);
                            });
                    }

                    return sharedCache.UserAccount
                        .InsertObject(EnterpriseHostApiBaseUriCacheKey, enterpriseHost.Address.ApiUri)
                        .Catch<Unit, Exception>(ex =>
                        {
                            log.Warn("Failed to persist enterprise host uri", ex);
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
            // informed of logins via the LoginComplete observable, and requests logins via
            // the RequiresLogin event.
            var connectionLogin = new ReplaySubject<IConnection>();
            connectionManager.LoginComplete = connectionLogin;
            Observable.FromEvent<IConnection>(
                h => connectionManager.RequiresLogin += h,
                h => connectionManager.RequiresLogin -= h
            ).Do(c =>
            {
                var address = c.HostAddress;
                var host = LookupHost(address);
                if (host == DisconnectedRepositoryHost)
                {
                    LogInFromCache(address)
                        .Do(x => connectionLogin.OnNext(c));
                }
            });

            // Wait until we've loaded (or failed to load) an enterprise uri from the db and then
            // start tracking changes to the EnterpriseHost property and persist every change to the db
            Observable.Concat(initialCacheLoadObs, persistEntepriseHostObs).Subscribe();
        }

        public IRepositoryHost LookupHost([AllowNull] HostAddress address)
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
            var isDotCom = HostAddress.GitHubDotComHostAddress == address;
            var host = RepositoryHostFactory.Create(address);
            return host.LogIn(usernameOrEmail, password)
                .Catch<AuthenticationResult, Exception>(Observable.Throw<AuthenticationResult>)
                .Do(result =>
                {
                    bool successful = result.IsSuccess();
                    log.Info(CultureInfo.InvariantCulture, "Log in to {3} host '{0}' with username '{1}' {2}",
                        address.ApiUri,
                        usernameOrEmail,
                        successful ? "SUCCEEDED" : "FAILED",
                        isDotCom ? "GitHub.com" : address.WebUri.Host
                    );
                    if (successful)
                    {
                        if (isDotCom)
                            GitHubHost = host;
                        else
                            EnterpriseHost = host;
                        connectionManager.AddConnection(address, usernameOrEmail);
                    }
                });
        }

        public IObservable<AuthenticationResult> LogInFromCache(HostAddress address)
        {
            var isDotCom = HostAddress.GitHubDotComHostAddress == address;
            var host = RepositoryHostFactory.Create(address);
            return host.LogInFromCache()
                .Catch<AuthenticationResult, Exception>(Observable.Throw<AuthenticationResult>)
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
        }

        public IObservable<Unit> LogOut(HostAddress address)
        {
            var host = LookupHost(address);
            return LogOut(host);
        }

        public IObservable<Unit> LogOut(IRepositoryHost host)
        {
            var address = host.Address;
            var isDotCom = HostAddress.GitHubDotComHostAddress == address;
            return host.LogOut()
                .Do(result =>
                {
                    // reset the logged out host property to null
                    // it'll know what to do
                    if (isDotCom)
                        GitHubHost = null;
                    else
                        EnterpriseHost = null;
                    connectionManager.RemoveConnection(address);
                });
        }

        IRepositoryHost githubHost;
        [AllowNull]
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
        [AllowNull]
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