using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    /// <summary>
    /// Provides services for logging into a GitHub server.
    /// </summary>
    public class LoginManager : ILoginManager
    {
        readonly string[] scopes = { "user", "repo", "gist", "write:public_key" };
        readonly ILoginCache loginCache;
        readonly ITwoFactorChallengeHandler twoFactorChallengeHandler;
        readonly string clientId;
        readonly string clientSecret;
        readonly string authorizationNote;
        readonly string fingerprint;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginManager"/> class.
        /// </summary>
        /// <param name="loginCache">The cache in which to store login details.</param>
        /// <param name="twoFactorChallengeHandler">The handler for 2FA challenges.</param>
        /// <param name="clientId">The application's client API ID.</param>
        /// <param name="clientSecret">The application's client API secret.</param>
        /// <param name="authorizationNote">An note to store with the authorization.</param>
        /// <param name="fingerprint">The machine fingerprint.</param>
        public LoginManager(
            ILoginCache loginCache,
            ITwoFactorChallengeHandler twoFactorChallengeHandler,
            string clientId,
            string clientSecret,
            string authorizationNote = null,
            string fingerprint = null)
        {
            Guard.ArgumentNotNull(loginCache, nameof(loginCache));
            Guard.ArgumentNotNull(twoFactorChallengeHandler, nameof(twoFactorChallengeHandler));
            Guard.ArgumentNotEmptyString(clientId, nameof(clientId));
            Guard.ArgumentNotEmptyString(clientSecret, nameof(clientSecret));

            this.loginCache = loginCache;
            this.twoFactorChallengeHandler = twoFactorChallengeHandler;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.authorizationNote = authorizationNote;
            this.fingerprint = fingerprint;
        }

        /// <inheritdoc/>
        public async Task<User> Login(
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
            await loginCache.SaveLogin(userName, password, hostAddress).ConfigureAwait(false);

            var newAuth = new NewAuthorization
            {
                Scopes = scopes,
                Note = authorizationNote,
                Fingerprint = fingerprint,
            };

            ApplicationAuthorization auth = null;

            do
            {
                try
                {
                    auth = await client.Authorization.Create(
                        clientId,
                        clientSecret,
                        newAuth).ConfigureAwait(false);
                    EnsureNonNullAuthorization(auth);
                }
                catch (TwoFactorAuthorizationException e)
                {
                    var challengeResult = await twoFactorChallengeHandler.HandleTwoFactorException(e);

                    if (challengeResult == null)
                    {
                        throw new InvalidOperationException(
                            "ITwoFactorChallengeHandler.HandleTwoFactorException returned null.");
                    }

                    if (!challengeResult.ResendCodeRequested)
                    {
                        auth = await SendTwoFactorAuorizationCode(
                            hostAddress,
                            client, 
                            newAuth,
                            challengeResult.AuthenticationCode).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    // Some enterpise instances don't support OAUTH, so fall back to using the
                    // supplied password - on intances that don't support OAUTH the user should
                    // be using a personal access token as the password.
                    if (EnterpriseWorkaround(hostAddress, e))
                    {
                        auth = new ApplicationAuthorization(password);
                    }
                    else
                    {
                        await loginCache.EraseLogin(hostAddress);
                        throw;
                    }
                }
            } while (auth == null);

            await loginCache.SaveLogin(userName, auth.Token, hostAddress).ConfigureAwait(false);
            return await client.User.Current().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<User> LoginFromCache(HostAddress hostAddress, IGitHubClient client)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNull(client, nameof(client));

            return client.User.Current();
        }

        /// <inheritdoc/>
        public async Task Logout(HostAddress hostAddress, IGitHubClient client)
        {
            Guard.ArgumentNotNull(hostAddress, nameof(hostAddress));
            Guard.ArgumentNotNull(client, nameof(client));

            await loginCache.EraseLogin(hostAddress);
        }

        async Task<ApplicationAuthorization> SendTwoFactorAuorizationCode(
            HostAddress hostAddress,
            IGitHubClient client,
            NewAuthorization newAuth,
            string authenticationCode)
        {
            try
            {
                var auth = await client.Authorization.Create(
                    clientId,
                    clientSecret,
                    newAuth,
                    authenticationCode).ConfigureAwait(false);
                return EnsureNonNullAuthorization(auth);
            }
            catch
            {
                await loginCache.EraseLogin(hostAddress);
                throw;
            }
        }

        ApplicationAuthorization EnsureNonNullAuthorization(ApplicationAuthorization auth)
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
    }
}
