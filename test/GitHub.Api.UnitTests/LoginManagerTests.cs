using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using NSubstitute;
using Octokit;
using NUnit.Framework;

public class LoginManagerTests
{
    static readonly HostAddress host = HostAddress.GitHubDotComHostAddress;
    static readonly HostAddress enterprise = HostAddress.Create("https://enterprise.hub");
    static readonly string[] scopes = { "user", "repo", "gist", "write:public_key" };

    public class TheLoginMethod
    {
        [Test]
        public async Task LoginTokenIsSavedToCache()
        {
            var client = CreateClient();
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns(new ApplicationAuthorization("123abc"));

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            await target.Login(host, client, "foo", "bar");

            await keychain.Received().Save("foo", "123abc", host);
        }

        [Test]
        public async Task LoggedInUserIsReturned()
        {
            var user = new User();
            var client = CreateClient(user);
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns(new ApplicationAuthorization("123abc"));

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            var result = await target.Login(host, client, "foo", "bar");

            Assert.That(user, Is.SameAs(result));
        }

        [Test]
        public async Task DeletesExistingAuthenticationIfNullTokenReturned()
        {
            // If GetOrCreateApplicationAuthentication is called and a matching token already exists,
            // the returned token will be null because it is assumed that the token will be stored
            // locally. In this case, the existing token should be first deleted.
            var client = CreateClient();
            var user = new User();
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns(
                    new ApplicationAuthorization(string.Empty),
                    new ApplicationAuthorization("123abc"));
            client.User.Current().Returns(user);

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            var result = await target.Login(host, client, "foo", "bar");

            await client.Authorization.Received(2).GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>());
            await client.Authorization.Received(1).Delete(0);
            await keychain.Received().Save("foo", "123abc", host);
        }

        [Test]
        public async Task TwoFactorExceptionIsPassedToHandler()
        {
            var client = CreateClient();
            var exception = new TwoFactorChallengeFailedException();

            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>(), "123456")
                .Returns(new ApplicationAuthorization("123abc"));

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();
            tfa.Value.HandleTwoFactorException(exception).Returns(new TwoFactorChallengeResult("123456"));

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            await target.Login(host, client, "foo", "bar");

            await client.Authorization.Received().GetOrCreateApplicationAuthentication(
                "id",
                "secret",
                Arg.Any<NewAuthorization>(),
                "123456");
        }

        [Test]
        public async Task Failed2FACodeResultsInRetry()
        {
            var client = CreateClient();
            var exception = new TwoFactorChallengeFailedException();

            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>(), "111111")
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>(), "123456")
                .Returns(new ApplicationAuthorization("123abc"));

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();
            tfa.Value.HandleTwoFactorException(exception).Returns(
                new TwoFactorChallengeResult("111111"),
                new TwoFactorChallengeResult("123456"));

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            await target.Login(host, client, "foo", "bar");

            await client.Authorization.Received(1).GetOrCreateApplicationAuthentication(
                "id",
                "secret",
                Arg.Any<NewAuthorization>(),
                "111111");
            await client.Authorization.Received(1).GetOrCreateApplicationAuthentication(
                "id",
                "secret",
                Arg.Any<NewAuthorization>(),
                "123456");
        }

        [Test]
        public async Task HandlerNotifiedOfExceptionIn2FAChallengeResponse()
        {
            var client = CreateClient();
            var twoFaException = new TwoFactorChallengeFailedException();
            var forbiddenResponse = Substitute.For<IResponse>();
            forbiddenResponse.StatusCode.Returns(HttpStatusCode.Forbidden);
            var loginAttemptsException = new LoginAttemptsExceededException(forbiddenResponse);

            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw twoFaException; });
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>(), "111111")
                .Returns<ApplicationAuthorization>(_ => { throw loginAttemptsException; });

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();
            tfa.Value.HandleTwoFactorException(twoFaException).Returns(
                new TwoFactorChallengeResult("111111"),
                new TwoFactorChallengeResult("123456"));

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            Assert.ThrowsAsync<LoginAttemptsExceededException>(async () => await target.Login(host, client, "foo", "bar"));

            await client.Authorization.Received(1).GetOrCreateApplicationAuthentication(
                "id",
                "secret",
                Arg.Any<NewAuthorization>(),
                "111111");
            await tfa.Value.Received(1).ChallengeFailed(loginAttemptsException);
        }

        [Test]
        public async Task RequestResendCodeResultsInRetryingLogin()
        {
            var client = CreateClient();
            var exception = new TwoFactorChallengeFailedException();
            var user = new User();

            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>(), "123456")
                .Returns(new ApplicationAuthorization("456def"));
            client.User.Current().Returns(user);

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();
            tfa.Value.HandleTwoFactorException(exception).Returns(
                TwoFactorChallengeResult.RequestResendCode,
                new TwoFactorChallengeResult("123456"));

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            await target.Login(host, client, "foo", "bar");

            await client.Authorization.Received(2).GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>());
        }

        [Test]
        public async Task UsesUsernameAndPasswordInsteadOfAuthorizationTokenWhenEnterpriseAndAPIReturns404()
        {
            var client = CreateClient();
            var user = new User();

            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ =>
                {
                    throw new NotFoundException("Not there", HttpStatusCode.NotFound);
                });
            client.User.Current().Returns(user);

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            await target.Login(enterprise, client, "foo", "bar");

            await keychain.Received().Save("foo", "bar", enterprise);
        }

        [Test]
        public async Task ErasesLoginWhenUnauthorized()
        {
            var client = CreateClient();
            var user = new User();

            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw new AuthorizationException(); });

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            Assert.ThrowsAsync<AuthorizationException>(async () => await target.Login(enterprise, client, "foo", "bar"));

            await keychain.Received().Delete(enterprise);
        }

        [Test]
        public async Task ErasesLoginWhenNonOctokitExceptionThrown()
        {
            var client = CreateClient();
            var user = new User();

            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw new InvalidOperationException(); });

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await target.Login(host, client, "foo", "bar"));


            await keychain.Received().Delete(host);
        }

        [Test]
        public async Task ErasesLoginWhenNonOctokitExceptionThrownIn2FA()
        {
            var client = CreateClient();
            var user = new User();
            var exception = new TwoFactorChallengeFailedException();

            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>(), "123456")
                .Returns<ApplicationAuthorization>(_ => { throw new InvalidOperationException(); });
            client.User.Current().Returns(user);

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();
            tfa.Value.HandleTwoFactorException(exception).Returns(new TwoFactorChallengeResult("123456"));

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await target.Login(host, client, "foo", "bar"));

            await keychain.Received().Delete(host);
        }

        [Test]
        public void InvalidResponseScopesCauseException()
        {
            var client = CreateClient(responseScopes: new[] { "user", "repo" });
            client.Authorization.GetOrCreateApplicationAuthentication("id", "secret", Arg.Any<NewAuthorization>())
                .Returns(new ApplicationAuthorization("123abc"));

            var keychain = Substitute.For<IKeychain>();
            var tfa = new Lazy<ITwoFactorChallengeHandler>(() => Substitute.For<ITwoFactorChallengeHandler>());
            var oauthListener = Substitute.For<IOAuthCallbackListener>();

            var target = new LoginManager(keychain, tfa, oauthListener, "id", "secret", scopes);

            Assert.ThrowsAsync<IncorrectScopesException>(() => target.Login(host, client, "foo", "bar"));
        }

        IGitHubClient CreateClient(User user = null, string[] responseScopes = null)
        {
            var result = Substitute.For<IGitHubClient>();
            var userResponse = Substitute.For<IApiResponse<User>>();
            userResponse.HttpResponse.Headers.Returns(new Dictionary<string, string>
            {
                {  "X-OAuth-Scopes", string.Join(",", responseScopes ?? scopes) }
            });
            userResponse.Body.Returns(user ?? new User());
            result.Connection.Get<User>(new Uri("user", UriKind.Relative), null, null).Returns(userResponse);
            return result;
        }
    }

    public class TheScopesMatchMethod
    {
        [Test]
        public void ReturnsFalseWhenMissingScopes()
        {
            var received = new[] { "user", "repo", "write:public_key" };

            Assert.False(LoginManager.ScopesMatch(scopes, received));
        }

        [Test]
        public void ReturnsTrueWhenScopesEqual()
        {
            var received = new[] { "user", "repo", "gist", "write:public_key" };

            Assert.True(LoginManager.ScopesMatch(scopes, received));
        }

        [Test]
        public void ReturnsTrueWhenExtraScopesReturned()
        {
            var received = new[] { "user", "repo", "gist", "foo", "write:public_key" };

            Assert.True(LoginManager.ScopesMatch(scopes, received));
        }

        [Test]
        public void ReturnsTrueWhenAdminScopeReturnedInsteadOfWrite()
        {
            var received = new[] { "user", "repo", "gist", "foo", "admin:public_key" };

            Assert.True(LoginManager.ScopesMatch(scopes, received));
        }
    }
}