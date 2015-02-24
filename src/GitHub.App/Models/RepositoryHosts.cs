using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Authentication;
using GitHub.Extensions.Reactive;
using ReactiveUI;

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
            GitHubHost = repositoryHostFactory.Create(HostAddress.GitHubDotComHostAddress);
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

        IRepositoryHost githubHost;
        public IRepositoryHost GitHubHost
        {
            get { return githubHost; }
            private set { this.RaiseAndSetIfChanged(ref githubHost, value); }
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

        public IObservable<AuthenticationResult> LogInEnterpriseHost(
            HostAddress enterpriseHostAddress,
            string usernameOrEmail,
            string password)
        {
            var host = RepositoryHostFactory.Create(enterpriseHostAddress);
            return host.LogIn(usernameOrEmail, password)
                .Catch<AuthenticationResult, Exception>(Observable.Throw<AuthenticationResult>)
                .Do(result =>
                {
                    bool successful = result.IsSuccess();
                    log.Info("Log in to Enterprise host '{0}' with username '{1}' {2}",
                        enterpriseHostAddress.ApiUri,
                        usernameOrEmail,
                        successful ? "SUCCEEDED" : "FAILED");
                    if (successful)
                    {
                        EnterpriseHost = host;
                    }
                });
        }

        public IObservable<AuthenticationResult> LogInGitHubHost(string usernameOrEmail, string password)
        {
            return GitHubHost.LogIn(usernameOrEmail, password)
                .Catch<AuthenticationResult, Exception>(Observable.Throw<AuthenticationResult>)
                .Do(result => log.Info("Log in to GitHub.com with username '{0}' {1}",
                    usernameOrEmail,
                    result.IsSuccess() ? "SUCCEEDED" : "FAILED"));
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