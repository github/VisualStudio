using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Caches;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Primitives;
using NLog;
using Octokit;
using ReactiveUI;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryHost : ReactiveObject, IRepositoryHost
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();
        static readonly User unverifiedUser = new User();

        bool isLoggedIn;
        bool isLoggingIn;
        readonly IAccountFactory accountFactory;

        public RepositoryHost(
            IApiClient apiClient,
            IHostCache hostCache,
            ILoginCache loginCache,
            IAccountFactory accountFactory)
        {
            Debug.Assert(apiClient.HostAddress != null, "HostAddress of an api client shouldn't be null");
            Address = apiClient.HostAddress;
            ApiBaseUri = apiClient.HostAddress.ApiUri;
            ApiClient = apiClient;
            Debug.Assert(ApiBaseUri != null, "Mistakes were made. ApiClient must have non-null ApiBaseUri");
            IsGitHub = ApiBaseUri.Equals(Api.ApiClient.GitHubDotComApiBaseUri);
            Cache = hostCache;
            LoginCache = loginCache;
            this.accountFactory = accountFactory;

            IsEnterprise = !IsGitHub;
            Title = MakeTitle(ApiBaseUri);
        }

        Uri ApiBaseUri { get; set; }

        public HostAddress Address { get; private set; }
        public IApiClient ApiClient { get; private set; }

        public IHostCache Cache { get; private set; }

        public bool IsGitHub { get; private set; }

        public bool IsEnterprise { get; private set; }

        public bool IsLoggedIn
        {
            get { return isLoggedIn; }
            private set { this.RaiseAndSetIfChanged(ref isLoggedIn, value); }
        }

        public bool IsLoggingIn
        {
            get { return isLoggingIn; }
            private set { this.RaiseAndSetIfChanged(ref isLoggingIn, value); }
        }

        public string Title { get; private set; }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IObservable<AuthenticationResult> LogInFromCache()
        {
            IsLoggingIn = true;

            return ApiClient.GetUser()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<User, Exception>(ex =>
                {
                    if (ex is AuthorizationException)
                    {
                        IsLoggingIn = false;
                        log.Warn("Got an authorization exception", ex);
                        return Observable.Return<User>(null);
                    }
                    return Cache.GetUser()
                        .Catch<User, Exception>(e =>
                        {
                            IsLoggingIn = false;
                            log.Warn("User does not exist in cache", e);
                            return Observable.Return<User>(null);
                        })
                        .ObserveOn(RxApp.MainThreadScheduler);
                })
                .SelectMany(LoginWithApiUser)
                .PublishAsync();
        }

        public IObservable<AuthenticationResult> LogIn(string usernameOrEmail, string password)
        {
            Guard.ArgumentNotEmptyString(usernameOrEmail, "usernameOrEmail");
            Guard.ArgumentNotEmptyString(password, "password");

            // If we need to retry on fallback, we'll store the 2FA token 
            // from the first request to re-use:
            string authenticationCode = null;

            // We need to intercept the 2FA handler to get the token:
            var interceptingTwoFactorChallengeHandler =
                new Func<TwoFactorRequiredException, IObservable<TwoFactorChallengeResult>>(ex =>
                    ApiClient.TwoFactorChallengeHandler.HandleTwoFactorException(ex)
                    .Do(twoFactorChallengeResult =>
                        authenticationCode = twoFactorChallengeResult.AuthenticationCode));

            // Keep the function to save the authorization token here because it's used
            // in multiple places in the chain below:
            var saveAuthorizationToken = new Func<ApplicationAuthorization, IObservable<Unit>>(authorization =>
            {
                if (authorization == null || String.IsNullOrWhiteSpace(authorization.Token))
                    return Observable.Return(Unit.Default);

                return LoginCache.SaveLogin(usernameOrEmail, authorization.Token, Address)
                    .ObserveOn(RxApp.MainThreadScheduler);
            });

            // Start be saving the username and password, as they will be used for older versions of Enterprise
            // that don't support authorization tokens, and for the API client to use until an authorization
            // token has been created and acquired:
            return LoginCache.SaveLogin(usernameOrEmail, password, Address)
                .Do(_ => IsLoggingIn = true)
                .ObserveOn(RxApp.MainThreadScheduler)
                // Try to get an authorization token, save it, then get the user to log in:
                .SelectMany(_ => ApiClient.GetOrCreateApplicationAuthenticationCode(interceptingTwoFactorChallengeHandler))
                .SelectMany(saveAuthorizationToken)
                .SelectMany(_ => ApiClient.GetUser())
                .Catch<User, ApiException>(firstTryEx =>
                {
                    var exception = firstTryEx as AuthorizationException;
                    if (IsEnterprise
                        && exception != null
                        && exception.Message == "Bad credentials")
                    {
                        return Observable.Throw<User>(exception);
                    }

                    // If the Enterprise host doesn't support the write:public_key scope, it'll return a 422.
                    // EXCEPT, there's a bug where it doesn't, and instead creates a bad token, and in 
                    // that case we'd get a 401 here from the GetUser invocation. So to be safe (and consistent
                    // with the Mac app), we'll just retry after any API error for Enterprise hosts:
                    if (IsEnterprise && !(firstTryEx is TwoFactorChallengeFailedException))
                    {
                        // Because we potentially have a bad authorization token due to the Enterprise bug,
                        // we need to reset to using username and password authentication:
                        return LoginCache.SaveLogin(usernameOrEmail, password, Address)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .SelectMany(_ =>
                            {
                                // Retry with the old scopes. If we have a stashed 2FA token, we use it:
                                if (authenticationCode != null)
                                    return ApiClient.GetOrCreateApplicationAuthenticationCode(
                                        authenticationCode,
                                        true);

                                // Otherwise, we use the default handler:
                                return ApiClient.GetOrCreateApplicationAuthenticationCode(useOldScopes: true);
                            })
                            // Then save the authorization token (if there is one) and get the user:
                            .SelectMany(saveAuthorizationToken)
                            .SelectMany(_ => ApiClient.GetUser());
                    }

                    return Observable.Throw<User>(firstTryEx);
                })
                .Catch<User, ApiException>(retryEx =>
                {
                    // Older Enterprise hosts either don't have the API end-point to PUT an authorization, or they
                    // return 422 because they haven't white-listed our client ID. In that case, we just ignore
                    // the failure, using basic authentication (with username and password) instead of trying
                    // to get an authorization token.
                    // Since enterprise 2.1 and https://github.com/github/github/pull/36669 the API returns 403
                    // instead of 404 to signal that it's not allowed. In the name of backwards compatibility we 
                    // test for both 404 (NotFoundException) and 403 (ForbiddenException) here.
                    if (IsEnterprise && (retryEx is NotFoundException || retryEx is ForbiddenException || retryEx.StatusCode == (HttpStatusCode)422))
                        return ApiClient.GetUser();

                    // Other errors are "real" so we pass them along:
                    return Observable.Throw<User>(retryEx);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<User, Exception>(ex =>
                {
                    // If we get here, we have an actual login failure:
                    IsLoggingIn = false;
                    if (ex is TwoFactorChallengeFailedException)
                    {
                        return Observable.Return(unverifiedUser);
                    }
                    if (ex is AuthorizationException)
                    {
                        return Observable.Return(default(User));
                    }
                    return Observable.Throw<User>(ex);
                })
                .SelectMany(LoginWithApiUser)
                .PublishAsync();
        }

        IObservable<AuthenticationResult> LoginWithApiUser(User user)
        {
            return Observable.Start(() =>
            {
                    if (user == null)
                    {
                        IsLoggingIn = false;
                        return AuthenticationResult.CredentialFailure;
                    }
                    if (user == unverifiedUser)
                    {
                        IsLoggingIn = false;
                        LoginCache.EraseLogin(Address);
                        return AuthenticationResult.VerificationFailure;
                    }

                    Cache.InsertUser(user);
                    IsLoggedIn = true;
                    IsLoggingIn = false;
                    return AuthenticationResult.Success;
                }, RxApp.MainThreadScheduler)
                .Do(result => log.Info("Log in from cache for login '{0}' to host '{1}' {2}",
                    user != null ? user.Login : "(null)",
                    ApiBaseUri,
                    result.IsSuccess() ? "SUCCEEDED" : "FAILED"))
                .PublishAsync();
        }

        public IObservable<Unit> LogOut()
        {
            if (!IsLoggedIn) return Observable.Return(Unit.Default);

            log.Info(CultureInfo.InvariantCulture, "Logged off of host '{0}'", ApiBaseUri);

            return LoginCache.EraseLogin(Address)
                .Catch<Unit, Exception>(e =>
                {
                    log.Warn("ASSERT! Failed to erase login. Going to invalidate cache anyways.", e);
                    return Observable.Return(Unit.Default);
                })
                .SelectMany(_ => Cache.InvalidateAll())
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

        public IObservable<IReadOnlyList<IAccount>> GetAccounts()
        {
            return Cache.GetUser().Select(acct => accountFactory.CreateAccount(this, acct))
                .Concat(Cache.GetAllOrganizations()
                    .SelectMany(orgs => orgs.Select(org => accountFactory.CreateAccount(this, org))))
                .ToList()
                .Select(accts => new ReadOnlyCollection<IAccount>(accts));
        }

        protected ILoginCache LoginCache { get; private set; }

        static string MakeTitle(Uri apiBaseUri)
        {
            return apiBaseUri.Equals(Api.ApiClient.GitHubDotComApiBaseUri) ?
                "github" :
                apiBaseUri.Host;
        }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, "RepositoryHost: {0} {1}", Title, ApiBaseUri);
            }
        }
    }
}
