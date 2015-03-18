using System;
using System.Collections.Generic;
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
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Primitives;
using GitHub.Services;
using NLog;
using Octokit;
using ReactiveUI;
using Authorization = Octokit.Authorization;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryHost : ReactiveObject, IRepositoryHost
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        bool isLoggedIn;
        bool isLoggingIn;
        bool isSelected;
        IAccount userAccount;
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
            Organizations = new ReactiveList<IAccount>();
            Title = MakeTitle(ApiBaseUri);

            Accounts = new ReactiveList<IAccount>();

            this.WhenAny(x => x.IsLoggedIn, x => x.Value)
                 .Where(loggedIn => loggedIn)
                 .Subscribe(_ => OnUserLoggedIn(User));

            this.WhenAny(x => x.IsLoggedIn, x => x.Value)
                .Skip(1) // so we don't log the account out on the initial evaluation of the WhenAny
                .Where(loggedIn => !loggedIn)
                .Subscribe(_ => OnUserLoggedOut());

            this.WhenAny(x => x.Organizations.ItemsAdded, x => x.Value)
                .SelectMany(x => x)
                .Subscribe(OnOrgAdded);

            this.WhenAny(x => x.Organizations.ItemsRemoved, x => x.Value)
                .SelectMany(x => x)
                .Subscribe(OnOrgRemoved);
        }

        Uri ApiBaseUri { get; set; }

        public HostAddress Address { get; private set; }
        public IApiClient ApiClient { get; private set; }

        public IHostCache Cache { get; private set; }

        public bool IsGitHub { get; private set; }

        public bool IsEnterprise { get; private set; }

        public bool IsLocal
        {
            get { return false; }
        }

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

        public bool IsSelected
        {
            get { return isSelected; }
            set { this.RaiseAndSetIfChanged(ref isSelected, value); }
        }

        public ReactiveList<IAccount> Organizations { get; private set; }

        public string Title { get; private set; }

        public IAccount User
        {
            get { return userAccount; }
            private set { this.RaiseAndSetIfChanged(ref userAccount, value); }
        }

        public ReactiveList<IAccount> Accounts { get; private set; }

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
                .Do(result =>
                {
                    if (result.IsFailure()) return;
                    AddCachedOrganizations();
                })
                .PublishAsync();
        }

        static readonly User unverifiedUser = new User();

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

                return LoginCache.SaveLogin(authorization.Token, "x-oauth-basic", Address)
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
                    if (IsEnterprise && (retryEx is NotFoundException || retryEx.StatusCode == (HttpStatusCode)422))
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
                .Do(result =>
                {
                    if (result.IsFailure()) return;
                    RefreshOrgs().Subscribe(
                        _ => { },
                        ex => log.Warn("Failed to refresh orgs.", ex));
                })
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
                    User = accountFactory.CreateAccount(this, user);
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

            log.Info(CultureInfo.InvariantCulture, "Logged user {0} off of host '{1}'", User.Login, ApiBaseUri);

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
                    Organizations.Clear();
                    User = null;
                });
        }

        public IObservable<Unit> Refresh()
        {
            return Refresh(h => Observable.Return(Unit.Default));
        }

        public IObservable<Unit> Refresh(Func<IRepositoryHost, IObservable<Unit>> refreshTrackedRepositoriesFunc)
        {
            if (!IsLoggedIn) return Observable.Return(Unit.Default);

            try
            {
                return Observable.Merge(
                    RefreshUser(),
                    RefreshOrgs())
                    .AsCompletion()
                    .SelectMany(_ => refreshTrackedRepositoriesFunc(this))
                    .Catch<Unit, Exception>(ex =>
                    {
                        log.Warn("Refresh failed.", ex);
                        return ex.ShowUserErrorMessage(ErrorType.RefreshFailed)
                            .AsCompletion();
                    });
            }
            catch (Exception ex)
            {
                log.Warn("Repository host refresh failed.", ex);
                return ex.ShowUserErrorMessage(ErrorType.RefreshFailed)
                    .AsCompletion();
            }
        }

        protected ILoginCache LoginCache { get; private set; }

        void AddCachedOrganizations()
        {
            Cache.GetAllOrganizations()
                .Select(orgs => orgs.Where(o => o != null))
                .Where(orgs => orgs.Any())
                .Select(orgs => orgs.OrderBy(org => org.Login))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    AddCachedOrganizations,
                    ex => log.Warn("Failed to get orgs from cache.", ex));
        }

        void AddCachedOrganizations(IEnumerable<Organization> ghOrgs)
        {
            if (ghOrgs == null) return;

            var orgs = ghOrgs.Select(ghOrg => accountFactory.CreateAccount(this, ghOrg))
                .Except(Organizations, (first, second) => first.Id == second.Id); // only add from cache if not already there

            // instead of AddRange here, we need to add the
            // items one at a time so the ItemAdded and ItemRemoved
            // signals are raised for the account list
            foreach (var org in orgs)
            {
                Organizations.Add(org);
            }
        }

        void AddOrUpdateOrg(IAccount org)
        {
            if (org == null) return;

            var existingOrg = Organizations.SingleOrDefault(x => x.Id == org.Id);
            if (existingOrg == null)
            {
                Organizations.Add(org);
                return;
            }
            Organizations[Organizations.IndexOf(existingOrg)] = org;
        }

        static string MakeTitle(Uri apiBaseUri)
        {
            return apiBaseUri.Equals(Api.ApiClient.GitHubDotComApiBaseUri) ?
                "github" :
                apiBaseUri.Host;
        }

        void RefreshOrgs(ICollection<IAccount> orgs)
        {
            orgs.ForEach(org =>
            {
                AddOrUpdateOrg(org);

                ApiClient.GetOrganization(org.Login)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(
                        ghOrg =>
                        {
                            Cache.InsertOrganization(ghOrg);
                            UpdateOrg(ghOrg);
                        },
                        ex => log.Warn("Failed to get organization.", ex));
            });

            var orgsToRemove = Organizations.Except(orgs, (x, y) => x.Id == y.Id).ToArray();

            orgsToRemove.ForEach(orgToRemove => Cache.InvalidateOrganization(orgToRemove));

            RemoveOrgs(orgsToRemove);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        IObservable<Unit> RefreshOrgs()
        {
            return ApiClient.GetOrganizations()
                .WhereNotNull()
                .ToArray()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(ghOrgs => ghOrgs
                    .OrderBy(ghOrg => ghOrg.Login)
                    .Select(org => accountFactory.CreateAccount(this, org))
                    .ToArray())
                .Select(ghOrgs =>
                {
                    RefreshOrgs(ghOrgs);
                    return Unit.Default;
                })
                .PublishAsync();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        IObservable<Unit> RefreshUser()
        {
            return ApiClient.GetUser()
                .WhereNotNull()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(ghUser =>
                {
                    Cache.InsertUser(ghUser);
                    if (User != null)
                        User.Update(ghUser);

                    return Unit.Default;
                })
                .PublishAsync();
        }

        void RemoveOrgs(IEnumerable<IAccount> orgs)
        {
            orgs.ForEach(org =>
            {
                var orgToRemove = Organizations.SingleOrDefault(o => org.Id == o.Id);
                if (orgToRemove != null)
                    Organizations.Remove(orgToRemove);
            });
        }

        void UpdateOrg(Organization ghOrg)
        {
            if (ghOrg == null) return;

            var existingOrg = Organizations.SingleOrDefault(x => x.Id == ghOrg.Id);
            if (existingOrg != null)
                existingOrg.Update(ghOrg);
        }


        void OnOrgAdded(IAccount account)
        {
            var accountTile = Accounts.SingleOrDefault(x => x != null && x.Id == account.Id);

            if (accountTile == null)
            {
                Accounts.Add(account);
            }
        }

        void OnOrgRemoved(IAccount account)
        {
            var accountTile = Accounts.SingleOrDefault(x => x != null && x.Id == account.Id);

            if (accountTile == null) return;

            Accounts.Remove(accountTile);
        }

        void OnUserLoggedIn(IAccount account)
        {
            Accounts.Insert(0, account);
        }

        void OnUserLoggedOut()
        {
            Accounts.Clear();
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
