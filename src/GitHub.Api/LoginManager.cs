using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;
using Serilog;

namespace GitHub.Api
{
    /// <summary>
    /// Provides services for logging into a GitHub server.
    /// </summary>
    public class LoginManager : ILoginManager
    {
        const string ScopesHeader = "X-OAuth-Scopes";
        static readonly ILogger log = LogManager.ForContext<LoginManager>();
        static readonly Uri UserEndpoint = new Uri("user", UriKind.Relative);
        readonly IKeychain keychain;
        readonly Lazy<ITwoFactorChallengeHandler> twoFactorChallengeHandler;
        readonly string clientId;
        readonly string clientSecret;
        readonly IReadOnlyList<string> minimumScopes;
        readonly IReadOnlyList<string> requestedScopes;
        readonly string authorizationNote;
        readonly string fingerprint;
        IOAuthCallbackListener oauthListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginManager"/> class.
        /// </summary>
        /// <param name="keychain">The keychain in which to store credentials.</param>
        /// <param name="twoFactorChallengeHandler">The handler for 2FA challenges.</param>
        /// <param name="oauthListener">The callback listener to signal successful login.</param>
        /// <param name="clientId">The application's client API ID.</param>
        /// <param name="clientSecret">The application's client API secret.</param>
        /// <param name="minimumScopes">The minimum acceptable scopes.</param>
        /// <param name="requestedScopes">The scopes to request when logging in.</param>
        /// <param name="authorizationNote">An note to store with the authorization.</param>
        /// <param name="fingerprint">The machine fingerprint.</param>
        public LoginManager(
            IKeychain keychain,
            Lazy<ITwoFactorChallengeHandler> twoFactorChallengeHandler,
            IOAuthCallbackListener oauthListener,
            string clientId,
            string clientSecret,
            IReadOnlyList<string> minimumScopes,
            IReadOnlyList<string> requestedScopes,
            string authorizationNote = null,
            string fingerprint = null)
        {
            Guard.ArgumentNotNull(keychain, nameof(keychain));
            Guard.ArgumentNotNull(twoFactorChallengeHandler, nameof(twoFactorChallengeHandler));
            Guard.ArgumentNotEmptyString(clientId, nameof(clientId));
            Guard.ArgumentNotEmptyString(clientSecret, nameof(clientSecret));

            this.keychain = keychain;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;
            this.oauthListener = oauthListener;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.minimumScopes = minimumScopes;
            this.requestedScopes = requestedScopes;
            this.authorizationNote = authorizationNote;
            this.fingerprint = fingerprint;
        }

        /// <inheritdoc/>
        public async Task<LoginResult> Login(
            HostAddress hostAddress,
            IGitHubClient client,
            string userName,
            string password)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNull(client, nameof(client));
            Guard.ArgumentNotEmptyString(userName, nameof(userName));
            Guard.ArgumentNotEmptyString(password, nameof(password));

            // Start by saving the username and password, these will be used by the `IGitHubClient`
            // until an authorization token has been created and acquired:
            await keychain.Save(userName, password, hostAddress).ConfigureAwait(false);

            var newAuth = new NewAuthorization
            {
                Scopes = requestedScopes,
                Note = authorizationNote,
                Fingerprint = fingerprint,
            };

            ApplicationAuthorization auth = null;

            do
            {
                try
                {
                    auth = await CreateAndDeleteExistingApplicationAuthorization(client, newAuth, null)
                        .ConfigureAwait(false);
                    EnsureNonNullAuthorization(auth);
                }
                catch (TwoFactorAuthorizationException e)
                {
                    auth = await HandleTwoFactorAuthorization(hostAddress, client, newAuth, e)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    // Some enterpise instances don't support OAUTH, so fall back to using the
                    // supplied password - on intances that don't support OAUTH the user should
                    // be using a personal access token as the password.
                    if (EnterpriseWorkaround(hostAddress, e))
                    {
                        auth = new ApplicationAuthorization(0, 
                            null, null, null, null, null, null, null,
                            DateTimeOffset.MinValue, DateTimeOffset.MinValue, null, password);
                    }
                    else
                    {
                        await keychain.Delete(hostAddress).ConfigureAwait(false);
                        throw;
                    }
                }
            } while (auth == null);

            await keychain.Save(userName, auth.Token, hostAddress).ConfigureAwait(false);
            return await ReadUserWithRetry(client).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<LoginResult> LoginViaOAuth(
            HostAddress hostAddress,
            IGitHubClient client,
            IOauthClient oauthClient,
            Action<Uri> openBrowser,
            CancellationToken cancel)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNull(client, nameof(client));
            Guard.ArgumentNotNull(oauthClient, nameof(oauthClient));
            Guard.ArgumentNotNull(openBrowser, nameof(openBrowser));

            var state = Guid.NewGuid().ToString();
            var loginUrl = GetLoginUrl(oauthClient, state);
            var listen = oauthListener.Listen(state, cancel);

            openBrowser(loginUrl);

            var code = await listen.ConfigureAwait(false);
            var request = new OauthTokenRequest(clientId, clientSecret, code);
            var token = await oauthClient.CreateAccessToken(request).ConfigureAwait(false);

            await keychain.Save("[oauth]", token.AccessToken, hostAddress).ConfigureAwait(false);
            var result = await ReadUserWithRetry(client).ConfigureAwait(false);
            await keychain.Save(result.User.Login, token.AccessToken, hostAddress).ConfigureAwait(false);
            return result;
        }

        /// <inheritdoc/>
        public async Task<LoginResult> LoginWithToken(
            HostAddress hostAddress,
            IGitHubClient client,
            string token)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNull(client, nameof(client));
            Guard.ArgumentNotEmptyString(token, nameof(token));

            await keychain.Save("[token]", token, hostAddress).ConfigureAwait(false);

            try
            {
                var result = await ReadUserWithRetry(client).ConfigureAwait(false);
                await keychain.Save(result.User.Login, token, hostAddress).ConfigureAwait(false);
                return result;
            }
            catch
            {
                await keychain.Delete(hostAddress).ConfigureAwait(false);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<LoginResult> LoginFromCache(HostAddress hostAddress, IGitHubClient client)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNull(client, nameof(client));

            return ReadUserWithRetry(client);
        }

        /// <inheritdoc/>
        public async Task Logout(HostAddress hostAddress, IGitHubClient client)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNull(client, nameof(client));

            await keychain.Delete(hostAddress).ConfigureAwait(false);
        }

        async Task<ApplicationAuthorization> CreateAndDeleteExistingApplicationAuthorization(
            IGitHubClient client,
            NewAuthorization newAuth,
            string twoFactorAuthenticationCode)
        {
            ApplicationAuthorization result;
            var retry = 0;

            do
            {
                if (twoFactorAuthenticationCode == null)
                {
                    result = await client.Authorization.GetOrCreateApplicationAuthentication(
                        clientId,
                        clientSecret,
                        newAuth).ConfigureAwait(false);
                }
                else
                {
                    result = await client.Authorization.GetOrCreateApplicationAuthentication(
                        clientId,
                        clientSecret,
                        newAuth,
                        twoFactorAuthenticationCode).ConfigureAwait(false);
                }

                if (string.IsNullOrEmpty(result.Token))
                {
                    if (twoFactorAuthenticationCode == null)
                    {
                        await client.Authorization.Delete(result.Id).ConfigureAwait(false);
                    }
                    else
                    {
                        await client.Authorization.Delete(result.Id, twoFactorAuthenticationCode).ConfigureAwait(false);
                    }
                }
            } while (string.IsNullOrEmpty(result.Token) && retry++ == 0);

            return result;
        }

        async Task<ApplicationAuthorization> HandleTwoFactorAuthorization(
            HostAddress hostAddress,
            IGitHubClient client,
            NewAuthorization newAuth,
            TwoFactorAuthorizationException exception)
        {
            for (;;)
            {
                var challengeResult = await twoFactorChallengeHandler.Value.HandleTwoFactorException(exception).ConfigureAwait(false);

                if (challengeResult == null)
                {
                    throw new InvalidOperationException(
                        "ITwoFactorChallengeHandler.HandleTwoFactorException returned null.");
                }

                if (!challengeResult.ResendCodeRequested)
                {
                    try
                    {
                        var auth = await CreateAndDeleteExistingApplicationAuthorization(
                            client,
                            newAuth,
                            challengeResult.AuthenticationCode).ConfigureAwait(false);
                        return EnsureNonNullAuthorization(auth);
                    }
                    catch (TwoFactorAuthorizationException e)
                    {
                        exception = e;
                    }
                    catch (Exception e)
                    {
                        await twoFactorChallengeHandler.Value.ChallengeFailed(e).ConfigureAwait(false);
                        await keychain.Delete(hostAddress).ConfigureAwait(false);
                        throw;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        static ApplicationAuthorization EnsureNonNullAuthorization(ApplicationAuthorization auth)
        {
            // If a mock IGitHubClient is not set up correctly, it can return null from
            // IGutHubClient.Authorization.Create - this will cause an infinite loop in Login()
            // so prevent that.
            if (auth == null)
            {
                throw new InvalidOperationException("IGutHubClient.Authorization.Create returned null.");
            }

            return auth;
        }

        bool EnterpriseWorkaround(HostAddress hostAddress, Exception e)
        {
            // Older Enterprise hosts either don't have the API end-point to PUT an authorization, or they
            // return 422 because they haven't white-listed our client ID. In that case, we just ignore
            // the failure, using basic authentication (with username and password) instead of trying
            // to get an authorization token.
            // Since enterprise 2.1 and https://github.com/github/github/pull/36669 the API returns 403
            // instead of 404 to signal that it's not allowed. In the name of backwards compatibility we 
            // test for both 404 (NotFoundException) and 403 (ForbiddenException) here.
            var apiException = e as ApiException;
            return !hostAddress.IsGitHubDotCom() &&
                (e is NotFoundException ||
                 e is ForbiddenException ||
                 apiException?.StatusCode == (HttpStatusCode)422);
        }

        async Task<LoginResult> ReadUserWithRetry(IGitHubClient client)
        {
            var retry = 0;

            while (true)
            {
                try
                {
                    return await GetUserAndCheckScopes(client).ConfigureAwait(false);
                }
                catch (AuthorizationException)
                {
                    if (retry++ == 3) throw;
                }

                // It seems that attempting to use a token immediately sometimes fails, retry a few
                // times with a delay of of 1s to allow the token to propagate.
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        async Task<LoginResult> GetUserAndCheckScopes(IGitHubClient client)
        {
            var response = await client.Connection.Get<User>(
                UserEndpoint, null, null).ConfigureAwait(false);

            if (response.HttpResponse.Headers.ContainsKey(ScopesHeader))
            {
                var returnedScopes = new ScopesCollection(response.HttpResponse.Headers[ScopesHeader]
                    .Split(',')
                    .Select(x => x.Trim())
                    .ToArray());

                if (returnedScopes.Matches(minimumScopes))
                {
                    return new LoginResult(response.Body, returnedScopes);
                }
                else
                {
                    log.Error("Incorrect API scopes: require {RequiredScopes} but got {Scopes}", minimumScopes, returnedScopes);
                }
            }
            else
            {
                log.Error("Error reading scopes: /user succeeded but scopes header was not present");
            }

            throw new IncorrectScopesException(
                "Incorrect API scopes. Required: " + string.Join(",", minimumScopes));
        }

        Uri GetLoginUrl(IOauthClient client, string state)
        {
            var request = new OauthLoginRequest(ApiClientConfiguration.ClientId);

            request.State = state;

            foreach (var scope in requestedScopes)
            {
                request.Scopes.Add(scope);
            }

            var uri = client.GetGitHubLoginUrl(request);
            
            // OauthClient.GetGitHubLoginUrl seems to give the wrong URL. Fix this.
            return new Uri(uri.ToString().Replace("/api/v3", ""));
        }
    }
}
