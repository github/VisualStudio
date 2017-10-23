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
using Xunit;

public class ConnectionManagerTests
{
    public class TheGetInitializedConnectionsMethod
    {
        [Fact]
        public async Task ReturnsValidConnections()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github", "valid"),
                Substitute.For<IKeychain>(),
                CreateLoginManager());
            var result = await target.GetLoadedConnections();

            Assert.Equal(2, result.Count);
            Assert.Equal("https://github.com/", result[0].HostAddress.WebUri.ToString());
            Assert.Equal("https://valid.com/", result[1].HostAddress.WebUri.ToString());
            Assert.Null(result[0].ConnectionError);
            Assert.Null(result[1].ConnectionError);
        }

        [Fact]
        public async Task ReturnsInvalidConnections()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github", "invalid"),
                Substitute.For<IKeychain>(),
                CreateLoginManager());
            var result = await target.GetLoadedConnections();

            Assert.Equal(2, result.Count);
            Assert.Equal("https://github.com/", result[0].HostAddress.WebUri.ToString());
            Assert.Equal("https://invalid.com/", result[1].HostAddress.WebUri.ToString());
            Assert.Null(result[0].ConnectionError);
            Assert.NotNull(result[1].ConnectionError);
        }
    }

    public class TheGetConnectionMethod
    {
        [Fact]
        public async Task ReturnsCorrectConnection()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github", "valid"),
                Substitute.For<IKeychain>(),
                CreateLoginManager());
            var result = await target.GetConnection(HostAddress.Create("valid.com"));

            Assert.Equal("https://valid.com/", result.HostAddress.WebUri.ToString());
        }

        [Fact]
        public async Task ReturnsCorrectNullForNotFoundConnection()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github", "valid"),
                Substitute.For<IKeychain>(),
                CreateLoginManager());
            var result = await target.GetConnection(HostAddress.Create("another.com"));

            Assert.Null(result);
        }
    }

    public class TheLoginMethod
    {
        [Fact]
        public async Task ReturnsLoggedInConnection()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache(),
                Substitute.For<IKeychain>(),
                CreateLoginManager());
            var result = await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task AddsLoggedInConnectionToConnections()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache(),
                Substitute.For<IKeychain>(),
                CreateLoginManager());

            await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass");

            Assert.Equal(1, target.Connections.Count);
        }

        [Fact]
        public async Task ThrowsWhenLoginFails()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache(),
                Substitute.For<IKeychain>(),
                CreateLoginManager());

            await Assert.ThrowsAsync<AuthorizationException>(async () =>
                await target.LogIn(HostAddress.Create("invalid.com"), "user", "pass"));
        }

        [Fact]
        public async Task ThrowsWhenExistingConnectionExists()
        {
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github"),
                Substitute.For<IKeychain>(),
                CreateLoginManager());

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass"));
        }

        [Fact]
        public async Task SavesConnectionToCache()
        {
            var cache = CreateConnectionCache();
            var target = new ConnectionManager(
                CreateProgram(),
                cache,
                Substitute.For<IKeychain>(),
                CreateLoginManager());

            await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass");

            await cache.Received(1).Save(Arg.Is<IEnumerable<ConnectionDetails>>(x =>
                x.Count() == 1 && x.ElementAt(0).HostAddress == HostAddress.Create("https://github.com")));
        }
    }

    public class TheLogOutMethod
    {
        [Fact]
        public async Task CallsLoginManagerLogOut()
        {
            var loginManager = CreateLoginManager();
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github"),
                Substitute.For<IKeychain>(),
                loginManager);

            await target.LogOut(HostAddress.GitHubDotComHostAddress);

            await loginManager.Received().Logout(
                HostAddress.GitHubDotComHostAddress,
                Arg.Any<IGitHubClient>());
        }

        [Fact]
        public async Task RemovesConnectionFromConnections()
        {
            var loginManager = CreateLoginManager();
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("github"),
                Substitute.For<IKeychain>(),
                loginManager);

            await target.LogOut(HostAddress.GitHubDotComHostAddress);

            Assert.Empty(target.Connections);
        }

        [Fact]
        public async Task ThrowsIfConnectionDoesntExist()
        {
            var loginManager = CreateLoginManager();
            var target = new ConnectionManager(
                CreateProgram(),
                CreateConnectionCache("valid"),
                Substitute.For<IKeychain>(),
                loginManager);

            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await target.LogOut(HostAddress.GitHubDotComHostAddress));
        }

        [Fact]
        public async Task RemovesConnectionFromCache()
        {
            var cache = CreateConnectionCache("github");
            var target = new ConnectionManager(
                CreateProgram(),
                cache,
                Substitute.For<IKeychain>(),
                CreateLoginManager());

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
