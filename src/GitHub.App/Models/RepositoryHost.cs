using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Extensions.Reactive;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;
using ReactiveUI;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Collections.Generic;
using GitHub.Extensions;
using GitHub.Logging;
using Serilog;
using ILoginCache = GitHub.Caches.ILoginCache;
using System.Threading.Tasks;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryHost : ReactiveObject, IRepositoryHost
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryHosts>();

        readonly ILoginManager loginManager;
        readonly HostAddress hostAddress;
        readonly ILoginCache loginCache;
        readonly IUsageTracker usage;

        bool isLoggedIn;

        public RepositoryHost(
            IApiClient apiClient,
            IModelService modelService,
            ILoginManager loginManager,
            ILoginCache loginCache,
            IUsageTracker usage)
        {
            ApiClient = apiClient;
            ModelService = modelService;
            this.loginManager = loginManager;
            this.loginCache = loginCache;
            this.usage = usage;

            log.Assert(apiClient.HostAddress != null, "HostAddress of an api client shouldn't be null");
            Address = apiClient.HostAddress;
            hostAddress = apiClient.HostAddress;
            Title = apiClient.HostAddress.Title;
        }

        public HostAddress Address { get; private set; }

        public IApiClient ApiClient { get; private set; }

        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            private set { this.RaiseAndSetIfChanged(ref isLoggedIn, value); }
        }

        public string Title { get; private set; }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IObservable<AuthenticationResult> LogInFromCache()
        {
            Func<Task<AuthenticationResult>> f = async () =>
            {
                try
                {
                    var user = await loginManager.LoginFromCache(Address, ApiClient.GitHubClient);
                    var accountCacheItem = new AccountCacheItem(user);

                    await usage.IncrementCounter(x => x.NumberOfLogins);
                    await ModelService.InsertUser(accountCacheItem);

                    if (user != null)
                    {
                        IsLoggedIn = true;
                        return AuthenticationResult.Success;
                    }
                    else
                    {
                        return AuthenticationResult.VerificationFailure;
                    }
                }
                catch (AuthorizationException)
                {
                    return AuthenticationResult.CredentialFailure;
                }
            };

            return f().ToObservable();
        }

        public IObservable<AuthenticationResult> LogIn(string usernameOrEmail, string password)
        {
            Guard.ArgumentNotEmptyString(usernameOrEmail, nameof(usernameOrEmail));
            Guard.ArgumentNotEmptyString(password, nameof(password));

            return Observable.Defer(async () =>
            {
                var user = await loginManager.Login(Address, ApiClient.GitHubClient, usernameOrEmail, password);
                var accountCacheItem = new AccountCacheItem(user);

                await usage.IncrementCounter(x => x.NumberOfLogins);
                await ModelService.InsertUser(accountCacheItem);

                if (user != null)
                {
                    IsLoggedIn = true;
                    return Observable.Return(AuthenticationResult.Success);
                }
                else
                {
                    return Observable.Return(AuthenticationResult.VerificationFailure);
                }
            });
        }

        public IObservable<Unit> LogOut()
        {
            if (!IsLoggedIn) return Observable.Return(Unit.Default);

            log.Information("Logged off of host '{ApiUri}'", hostAddress.ApiUri);

            return loginCache.EraseLogin(Address)
                .Catch<Unit, Exception>(e =>
                {
                    log.Warning(e, "ASSERT! Failed to erase login. Going to invalidate cache anyways");
                    return Observable.Return(Unit.Default);
                })
                .SelectMany(_ => ModelService.InvalidateAll())
                .Catch<Unit, Exception>(e =>
                {
                    log.Warning(e, "ASSERT! Failed to invaldiate caches");
                    return Observable.Return(Unit.Default);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Finally(() =>
                {
                    IsLoggedIn = false;
                });
        }

        protected virtual void Dispose(bool disposing)
        {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "RepositoryHost: {0} {1}", Title, hostAddress.ApiUri);
            }
        }

        public IModelService ModelService
        {
            get;
            private set;
        }
    }
}
