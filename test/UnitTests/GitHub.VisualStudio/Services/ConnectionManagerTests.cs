using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using NSubstitute;
using Octokit;
using NUnit.Framework;

public class ConnectionManagerTests
{
    public class TheGetInitializedConnectionsMethod
    {
        [Test]
        public async Task ReturnsValidConnections()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github", "valid"),
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());
            var result = await target.GetLoadedConnections();

            Assert.That(2, Is.EqualTo(result.Count));
            Assert.That("https://github.com/", Is.EqualTo(result[0].HostAddress.WebUri.ToString()));
            Assert.That("https://valid.com/", Is.EqualTo(result[1].HostAddress.WebUri.ToString()));
            Assert.That(result[0].ConnectionError, Is.Null);
            Assert.That(result[1].ConnectionError, Is.Null);
        }

        [Test]
        public async Task ReturnsInvalidConnections()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github", "invalid"),
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());
            var result = await target.GetLoadedConnections();

            Assert.That(2, Is.EqualTo(result.Count));
            Assert.That("https://github.com/",Is.EqualTo(result[0].HostAddress.WebUri.ToString()));
            Assert.That("https://invalid.com/", Is.EqualTo(result[1].HostAddress.WebUri.ToString()));
            Assert.That(result[0].ConnectionError, Is.Null);
            Assert.That(result[1].ConnectionError, Is.Not.Null);
        }
    }

    public class TheGetConnectionMethod
    {
        [Test]
        public async Task ReturnsCorrectConnection()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github", "valid"),
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());
            var result = await target.GetConnection(HostAddress.Create("valid.com"));

            Assert.That("https://valid.com/", Is.EqualTo(result.HostAddress.WebUri.ToString()));
        }

        [Test]
        public async Task ReturnsCorrectNullForNotFoundConnection()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github", "valid"),
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());
            var result = await target.GetConnection(HostAddress.Create("another.com"));

            Assert.That(result, Is.Null);
        }
    }

    public class TheLoginMethod
    {
        [Test]
        public async Task ReturnsLoggedInConnection()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache(),
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());
            var result = await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass");

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task AddsLoggedInConnectionToConnections()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache(),
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());

            await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass");

            Assert.That(1, Is.EqualTo(target.Connections.Count));
        }

        [Test]
        public async Task ThrowsWhenLoginFails()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache(),
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());

            Assert.ThrowsAsync<AuthorizationException>(async () =>
                await target.LogIn(HostAddress.Create("invalid.com"), "user", "pass"));
        }

        [Test]
        public async Task ThrowsWhenExistingConnectionExists()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github"),
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass"));
        }

        [Test]
        public async Task SavesConnectionToCache()
        {
            var cache = CreateConnectionCache();
            var target = new ConnectionManager(
                CreateProgram(),
                cache,
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());

            await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass");

            await cache.Received(1).Save(Arg.Is<IEnumerable<ConnectionDetails>>(x =>
                x.Count() == 1 && x.ElementAt(0).HostAddress == HostAddress.Create("https://github.com")));
        }
    }

    public class TheLogOutMethod
    {
        [Test]
        public async Task CallsLoginManagerLogOut()
        {
            var loginManager = CreateLoginManager();
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github"),
                Substitute.For<IKeychain>(),
                loginManager,
                Substitute.For<IUsageTracker>());

            await target.LogOut(HostAddress.GitHubDotComHostAddress);

            await loginManager.Received().Logout(
                HostAddress.GitHubDotComHostAddress,
                Arg.Any<IGitHubClient>());
        }

        [Test]
        public async Task RemovesConnectionFromConnections()
        {
            var loginManager = CreateLoginManager();
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github"),
                Substitute.For<IKeychain>(),
                loginManager,
                Substitute.For<IUsageTracker>());

            await target.LogOut(HostAddress.GitHubDotComHostAddress);

            Assert.That(target.Connections, Is.Empty);
        }

        [Test]
        public async Task ThrowsIfConnectionDoesntExist()
        {
            var loginManager = CreateLoginManager();
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("valid"),
                Substitute.For<IKeychain>(),
                loginManager,
                Substitute.For<IUsageTracker>());

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await target.LogOut(HostAddress.GitHubDotComHostAddress));
        }

        [Test]
        public async Task RemovesConnectionFromCache()
        {
            var cache = CreateConnectionCache("github");
            var target = new ConnectionManager(
                CreateProgram(),
                cache,
                Substitute.For<IKeychain>(),
                CreateLoginManager(),
                Substitute.For<IUsageTracker>());

            await target.LogOut(HostAddress.GitHubDotComHostAddress);

            await cache.Received(1).Save(Arg.Is<IEnumerable<ConnectionDetails>>(x => x.Count() == 0));
        }
    }

    static IProgram CreateProgram()
    {
        var result = Substitute.For<IProgram>();
        result.ProductHeader.Returns(new ProductHeaderValue("foo"));
        return result;
    }

    static IConnectionCache CreateConnectionCache(params string[] hosts)
    {
        var result = Substitute.For<IConnectionCache>();
        var details = hosts.Select(x => new ConnectionDetails(
            HostAddress.Create($"https://{x}.com"),
            "user"));
        result.Load().Returns(details.ToList());
        return result;
    }

    static ILoginManager CreateLoginManager()
    {
        var result = Substitute.For<ILoginManager>();
        result.Login(HostAddress.Create("invalid.com"), Arg.Any<IGitHubClient>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns<User>(_ => { throw new AuthorizationException(); });
        result.LoginFromCache(HostAddress.Create("invalid.com"), Arg.Any<IGitHubClient>())
            .Returns<User>(_ => { throw new AuthorizationException(); });
        return result;
    }
}
