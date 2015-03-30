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

namespace GitHub.Models
{
    [Export(typeof(IRepositoryHosts))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryHosts : ReactiveObject, IRepositoryHosts, IDisposable
    {
        static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static DisconnectedRepositoryHost DisconnectedRepositoryHost = new DisconnectedRepositoryHost();
        public const string EnterpriseHostApiBaseUriCacheKey = "enterprise-host-api-base-uri";
        readonly ObservableAsPropertyHelper<bool> isLoggedInToAnyHost;

        [ImportingConstructor]
        public RepositoryHosts(
            IRepositoryHostFactory repositoryHostFactory,
            ISharedCache sharedCache)
        {
            RepositoryHostFactory = repositoryHostFactory;

            LocalRepositoriesHost = new LocalRepositoriesHost();
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

            // Wait until we've loaded (or failed to load) an enterprise uri from the db and then
            // start tracking changes to the EntepriseHost property and persist every change to the db
            Observable.Concat(initialCacheLoadObs, persistEntepriseHostObs).Subscribe();
        }

        public IRepositoryHost LookupHost(HostAddress address)
        {
            if (address == GitHubHost.Address)
                return GitHubHost;
            if (address == EnterpriseHost.Address)
                return EnterpriseHost;
            return DisconnectedRepositoryHost;
        }

        IRepositoryHost githubHost;
        public IRepositoryHost GitHubHost
        {
            get { return githubHost; }
            private set {
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

        IRepositoryHost localRepositoriesHost;
        public IRepositoryHost LocalRepositoriesHost
        {
            get { return localRepositoriesHost; }
            set { this.RaiseAndSetIfChanged(ref localRepositoriesHost, value); }
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
                        isDotCom ? "GitHub.com" : "Enterprise"
                    );
                    if (successful)
                    {
                        if (isDotCom)
                            EnterpriseHost = host;
                        else
                            GitHubHost = host;
                    }
                });
        }

        public IRepositoryHostFactory RepositoryHostFactory { get; private set; }

        public bool IsLoggedInToAnyHost { get { return isLoggedInToAnyHost.Value; } }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && EnterpriseHost.Cache != null)
            {
                EnterpriseHost.Cache.Dispose();
            }
        }
    }
}