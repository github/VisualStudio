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
using NLog;
using Octokit;
using ReactiveUI;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryHost : ReactiveObject, IRepositoryHost
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        static readonly AccountCacheItem unverifiedUser = new AccountCacheItem();

        readonly ITwoFactorChallengeHandler twoFactorChallengeHandler;
        readonly HostAddress hostAddress;
        readonly ILoginCache loginCache;

        bool isLoggedIn;
        readonly bool isEnterprise;

        public RepositoryHost(
            IApiClient apiClient,
            IModelService modelService,
            ILoginCache loginCache,
            ITwoFactorChallengeHandler twoFactorChallengeHandler)
        {
            ApiClient = apiClient;
            ModelService = modelService;
            this.loginCache = loginCache;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;

            Debug.Assert(apiClient.HostAddress != null, "HostAddress of an api client shouldn't be null");
            Address = apiClient.HostAddress;
            hostAddress = apiClient.HostAddress;
            isEnterprise = !hostAddress.IsGitHubDotCom();
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
            return GetUserFromApi()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<AccountCacheItem, Exception>(ex =>
                {
                    if (ex is AuthorizationException)
                    {
                        log.Warn("Got an authorization exception", ex);
                        return Observable.Return<AccountCacheItem>(null);
                    }
                    return ModelService.GetUserFromCache()
                        .Catch<AccountCacheItem, Exception>(e =>
                        {
                            log.Warn("User does not exist in cache", e);
                            return Observable.Return<AccountCacheItem>(null);
                        })
                        .ObserveOn(RxApp.MainThreadScheduler);
                })
                .SelectMany(LoginWithApiUser)
                .PublishAsync();
        }

        public IObservable<AuthenticationResult> LogIn(string usernameOrEmail, string password)
        {
            Guard.ArgumentNotEmptyString(usernameOrEmail, nameof(usernameOrEmail));
            Guard.ArgumentNotEmptyString(password, nameof(password));

            // If we need to retry on fallback, we'll store the 2FA token 
            // from the first request to re-use:
            string authenticationCode = null;

            // We need to intercept the 2FA handler to get the token:
            var interceptingTwoFactorChallengeHandler =
                new Func<TwoFactorAuthorizationException, IObservable<TwoFactorChallengeResult>>(ex =>
                    twoFactorChallengeHandler.HandleTwoFactorException(ex)
                    .Do(twoFactorChallengeResult =>
                        authenticationCode = twoFactorChallengeResult.AuthenticationCode));

            // Keep the function to save the authorization token here because it's used
            // in multiple places in the chain below:
            var saveAuthorizationToken = new Func<ApplicationAuthorization, IObservable<Unit>>(authorization =>
            {
                var token = authorization?.Token;
                if (string.IsNullOrWhiteSpace(token))
                    return Observable.Return(Unit.Default);

                return loginCache.SaveLogin(usernameOrEmail, token, Address)
                    .ObserveOn(RxApp.MainThreadScheduler);
            });

            // Start be saving the username and password, as they will be used for older versions of Enterprise
            // that don't support authorization tokens, and for the API client to use until an authorization
            // token has been created and acquired:
            return loginCache.SaveLogin(usernameOrEmail, password, Address)
                .ObserveOn(RxApp.MainThreadScheduler)
                // Try to get an authorization token, save it, then get the user to log in:
                .SelectMany(fingerprint => ApiClient.GetOrCreateApplicationAuthenticationCode(interceptingTwoFactorChallengeHandler))
                .SelectMany(saveAuthorizationToken)
                .SelectMany(_ => GetUserFromApi())
                .Catch<AccountCacheItem, ApiException>(firstTryEx =>
                {
                    var exception = firstTryEx as AuthorizationException;
                    if (isEnterprise
                        && exception != null
                        && exception.Message == "Bad credentials")
                    {
                        return Observable.Throw<AccountCacheItem>(exception);
                    }

                    // If the Enterprise host doesn't support the write:public_key scope, it'll return a 422.
                    // EXCEPT, there's a bug where it doesn't, and instead creates a bad token, and in 
                    // that case we'd get a 401 here from the GetUser invocation. So to be safe (and consistent
                    // with the Mac app), we'll just retry after any API error for Enterprise hosts:
                    if (isEnterprise && !(firstTryEx is TwoFactorChallengeFailedException))
                    {
                        // Because we potentially have a bad authorization token due to the Enterprise bug,
                        // we need to reset to using username and password authentication:
                        return loginCache.SaveLogin(usernameOrEmail, password, Address)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .SelectMany(_ =>
                            {
                                // Retry with the old scopes. If we have a stashed 2FA token, we use it:
                                if (authenticationCode != null)
                                {
                                    return ApiClient.GetOrCreateApplicationAuthenticationCode(
                                        interceptingTwoFactorChallengeHandler,
                                        authenticationCode,
                                        useOldScopes: true,
                                        useFingerprint: false);
                                }

                                // Otherwise, we use the default handler:
                                return ApiClient.GetOrCreateApplicationAuthenticationCode(
                                    interceptingTwoFactorChallengeHandler,
                                    useOldScopes: true,
                                    useFingerprint: false);
                            })
                            // Then save the authorization token (if there is one) and get the user:
                            .SelectMany(saveAuthorizationToken)
                            .SelectMany(_ => GetUserFromApi());
                    }

                    return Observable.Throw<AccountCacheItem>(firstTryEx);
                })
                .Catch<AccountCacheItem, ApiException>(retryEx =>
                {
                    // Older Enterprise hosts either don't have the API end-point to PUT an authorization, or they
                    // return 422 because they haven't white-listed our client ID. In that case, we just ignore
                    // the failure, using basic authentication (with username and password) instead of trying
                    // to get an authorization token.
                    // Since enterprise 2.1 and https://github.com/github/github/pull/36669 the API returns 403
                    // instead of 404 to signal that it's not allowed. In the name of backwards compatibility we 
                    // test for both 404 (NotFoundException) and 403 (ForbiddenException) here.
                    if (isEnterprise && (retryEx is NotFoundException || retryEx is ForbiddenException || retryEx.StatusCode == (HttpStatusCode)422))
                        return GetUserFromApi();

                    // Other errors are "real" so we pass them along:
                    return Observable.Throw<AccountCacheItem>(retryEx);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<AccountCacheItem, Exception>(ex =>
                {
                    // If we get here, we have an actual login failure:
                    if (ex is TwoFactorChallengeFailedException)
                    {
                        return Observable.Return(unverifiedUser);
                    }
                    if (ex is AuthorizationException)
                    {
                        return Observable.Return(default(AccountCacheItem));
                    }
                    return Observable.Throw<AccountCacheItem>(ex);
                })
                .SelectMany(LoginWithApiUser)
                .PublishAsync();
        }

        public IObservable<Unit> LogOut()
        {
            if (!IsLoggedIn) return Observable.Return(Unit.Default);

            log.Info(CultureInfo.InvariantCulture, "Logged off of host '{0}'", hostAddress.ApiUri);

            return loginCache.EraseLogin(Address)
                .Catch<Unit, Exception>(e =>
                {
                    log.Warn("ASSERT! Failed to erase login. Going to invalidate cache anyways.", e);
                    return Observable.Return(Unit.Default);
                })
                .SelectMany(_ => ModelService.InvalidateAll())
                .Catch<Unit, Exception>(e =>
                {
                    log.Warn("ASSERT! Failed to invaldiate caches", e);
                    return Observable.Return(Unit.Default);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Finally(() =>
                {
                    IsLoggedIn = false;
                });
        }

        static IObservable<AuthenticationResult> GetAuthenticationResultForUser(AccountCacheItem account)
        {
            return Observable.Return(account == null ? AuthenticationResult.CredentialFailure
                : account == unverifiedUser
                    ? AuthenticationResult.VerificationFailure
                    : AuthenticationResult.Success);
        }

        IObservable<AuthenticationResult> LoginWithApiUser(AccountCacheItem user)
        {
            return GetAuthenticationResultForUser(user)
                .SelectMany(result =>
                {
                    if (result.IsSuccess())
                    {
                        return ModelService.InsertUser(user).Select(_ => result);
                    }

                    if (result == AuthenticationResult.VerificationFailure)
                    {
                        return loginCache.EraseLogin(Address).Select(_ => result);
                    }
                    return Observable.Return(result);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(result =>
                {
                    if (result.IsSuccess())
                    {
                        IsLoggedIn = true;
                    }

                    log.Info("Log in from cache for login '{0}' to host '{1}' {2}",
                        user != null ? user.Login : "(null)",
                        hostAddress.ApiUri,
                        result.IsSuccess() ? "SUCCEEDED" : "FAILED");
                });
        }

        IObservable<AccountCacheItem> GetUserFromApi()
        {
            return Observable.Defer(() => ApiClient.GetUser().WhereNotNull()
                .Select(user => new AccountCacheItem(user)));
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                try
                {
                    ModelService.Dispose();
                }
                catch (Exception e)
                {
                    log.Warn("Exception occured while disposing RepositoryHost's ModelService", e);
                }
                disposed = true;
            }
        }

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
