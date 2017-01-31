using System;
using System.Net;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using NSubstitute;
using Octokit;
using Xunit;

public class LoginManagerTests
{
    static readonly HostAddress host = HostAddress.GitHubDotComHostAddress;
    static readonly HostAddress enterprise = HostAddress.Create("https://ghe.io");

    public class TheLoginMethod
    {
        [Fact]
        public async Task LoginTokenIsSavedToCache()
        {
            var client = Substitute.For<IGitHubClient>();
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns(new ApplicationAuthorization("123abc"));

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await target.Login(host, client, "foo", "bar");

            await loginCache.Received().SaveLogin("foo", "123abc", host);
        }

        [Fact]
        public async Task LoggedInUserIsReturned()
        {
            var client = Substitute.For<IGitHubClient>();
            var user = new User();
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns(new ApplicationAuthorization("123abc"));
            client.User.Current().Returns(user);

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            var result = await target.Login(host, client, "foo", "bar");

            Assert.Same(user, result);
        }

        [Fact]
        public async Task TwoFactorExceptionIsPassedToHandler()
        {
            var client = Substitute.For<IGitHubClient>();
            var exception = new TwoFactorChallengeFailedException();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>(), "def567")
                .Returns(new ApplicationAuthorization("123abc"));

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();
            tfa.HandleTwoFactorException(exception).Returns(new TwoFactorChallengeResult("def567"));

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await target.Login(host, client, "foo", "bar");
        }

        [Fact]
        public async Task RequestResendCodeResultsInRetryingLogin()
        {
            var client = Substitute.For<IGitHubClient>();
            var exception = new TwoFactorChallengeFailedException();
            var user = new User();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>(), "def567")
                .Returns(new ApplicationAuthorization("456def"));
            client.User.Current().Returns(user);

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();
            tfa.HandleTwoFactorException(exception).Returns(
                TwoFactorChallengeResult.RequestResendCode,
                new TwoFactorChallengeResult("def567"));

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await target.Login(host, client, "foo", "bar");

            await client.Authorization.Received(2).Create("id", "secret", Arg.Any<NewAuthorization>());
        }

        [Fact]
        public async Task UsesUsernameAndPasswordInsteadOfAuthorizationTokenWhenEnterpriseAndAPIReturns404()
        {
            var client = Substitute.For<IGitHubClient>();
            var user = new User();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => 
                {
                    throw new NotFoundException("Not there", HttpStatusCode.NotFound);
                });
            client.User.Current().Returns(user);

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await target.Login(enterprise, client, "foo", "bar");

            await loginCache.Received().SaveLogin("foo", "bar", enterprise);
        }

        [Fact]
        public async Task ErasesLoginWhenUnauthorized()
        {
            var client = Substitute.For<IGitHubClient>();
            var user = new User();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw new AuthorizationException(); });

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await Assert.ThrowsAsync<AuthorizationException>(async () => await target.Login(enterprise, client, "foo", "bar"));

            await loginCache.Received().EraseLogin(enterprise);
        }

        [Fact]
        public async Task ErasesLoginWhenNonOctokitExceptionThrown()
        {
            var client = Substitute.For<IGitHubClient>();
            var user = new User();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw new InvalidOperationException(); });

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await target.Login(host, client, "foo", "bar"));

            await loginCache.Received().EraseLogin(host);
        }

        [Fact]
        public async Task ErasesLoginWhenBad2FAEntered()
        {
            var client = Substitute.For<IGitHubClient>();
            var user = new User();
            var exception = new TwoFactorChallengeFailedException();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>(), "def567")
                .Returns<ApplicationAuthorization>(_ => { throw new TwoFactorChallengeFailedException(); });
            client.User.Current().Returns(user);

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();
            tfa.HandleTwoFactorException(exception).Returns(new TwoFactorChallengeResult("def567"));

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await Assert.ThrowsAsync<TwoFactorChallengeFailedException>(async () => await target.Login(host, client, "foo", "bar"));

            await loginCache.Received().EraseLogin(host);
        }

        [Fact]
        public async Task ErasesLoginWhenNonOctokitExceptionThrownIn2FA()
        {
            var client = Substitute.For<IGitHubClient>();
            var user = new User();
            var exception = new TwoFactorChallengeFailedException();

            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>())
                .Returns<ApplicationAuthorization>(_ => { throw exception; });
            client.Authorization.Create("id", "secret", Arg.Any<NewAuthorization>(), "def567")
                .Returns<ApplicationAuthorization>(_ => { throw new InvalidOperationException(); });
            client.User.Current().Returns(user);

            var loginCache = Substitute.For<ILoginCache>();
            var tfa = Substitute.For<ITwoFactorChallengeHandler>();
            tfa.HandleTwoFactorException(exception).Returns(new TwoFactorChallengeResult("def567"));

            var target = new LoginManager(loginCache, tfa, "id", "secret");
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await target.Login(host, client, "foo", "bar"));

            await loginCache.Received().EraseLogin(host);
        }
    }
}
