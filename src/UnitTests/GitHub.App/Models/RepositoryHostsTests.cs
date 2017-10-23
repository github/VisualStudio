using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using Xunit;

namespace UnitTests.GitHub.App.Models
{
    public class RepositoryHostsTests
    {
        static readonly HostAddress EnterpriseHostAddress = HostAddress.Create("https://enterprise.host");
        static readonly HostAddress InvalidHostAddress = HostAddress.Create("https://invalid.host");

        public class TheGitHubHostProperty
        {
            [Fact]
            public void ShouldInitiallyBeDisconnectedRepositoryHost()
            {
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    CreateConnectionManager());

                Assert.Same(RepositoryHosts.DisconnectedRepositoryHost, target.GitHubHost);
            }

            [Fact]
            public void ShouldCreateHostForExistingConnection()
            {
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    CreateConnectionManager(HostAddress.GitHubDotComHostAddress));

                Assert.NotSame(RepositoryHosts.DisconnectedRepositoryHost, target.GitHubHost);
            }

            [Fact]
            public async Task ShouldCreateHostWhenConnectionLoggedIn()
            {
                var connectionManager = CreateConnectionManager();
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    connectionManager);

                await connectionManager.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass");

                Assert.NotSame(RepositoryHosts.DisconnectedRepositoryHost, target.GitHubHost);
            }

            [Fact]
            public async Task ShouldRemoveHostWhenConnectionLoggedOut()
            {
                var connectionManager = CreateConnectionManager(HostAddress.GitHubDotComHostAddress);
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    connectionManager);

                await connectionManager.LogOut(HostAddress.GitHubDotComHostAddress);

                Assert.Same(RepositoryHosts.DisconnectedRepositoryHost, target.GitHubHost);
            }
        }

        public class TheEnterpriseHostProperty
        {
            [Fact]
            public void ShouldInitiallyBeDisconnectedRepositoryHost()
            {
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    CreateConnectionManager());

                Assert.Same(RepositoryHosts.DisconnectedRepositoryHost, target.EnterpriseHost);
            }

            [Fact]
            public void ShouldCreateHostForExistingConnection()
            {
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    CreateConnectionManager(EnterpriseHostAddress));

                Assert.NotSame(RepositoryHosts.DisconnectedRepositoryHost, target.EnterpriseHost);
            }

            [Fact]
            public async Task ShouldCreateHostWhenConnectionLoggedIn()
            {
                var connectionManager = CreateConnectionManager();
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    connectionManager);

                await connectionManager.LogIn(EnterpriseHostAddress, "user", "pass");

                Assert.NotSame(RepositoryHosts.DisconnectedRepositoryHost, target.EnterpriseHost);
            }

            [Fact]
            public async Task ShouldRemoveHostWhenConnectionLoggedOut()
            {
                var connectionManager = CreateConnectionManager(EnterpriseHostAddress);
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    connectionManager);

                await connectionManager.LogOut(EnterpriseHostAddress);

                Assert.Same(RepositoryHosts.DisconnectedRepositoryHost, target.EnterpriseHost);
            }
        }

        public class TheLoginMethod
        {
            [Fact]
            public async Task ShouldCreateGitHubHost()
            {
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    CreateConnectionManager());

                await target.LogIn(HostAddress.GitHubDotComHostAddress, "user", "pass");

                Assert.NotSame(RepositoryHosts.DisconnectedRepositoryHost, target.GitHubHost);
            }

            [Fact]
            public async Task ShouldFailForInvalidHost()
            {
                var target = new RepositoryHosts(
                    CreateRepositoryHostFactory(),
                    CreateConnectionManager());

                await Assert.ThrowsAsync<Exception>(async () =>
                {
                    await target.LogIn(InvalidHostAddress, "user", "pass");
                });

                Assert.Same(RepositoryHosts.DisconnectedRepositoryHost, target.GitHubHost);
            }
        }

        static IConnectionManager CreateConnectionManager(params HostAddress[] hostAddresses)
        {
            var result = Substitute.For<IConnectionManager>();
            var connectionSource = hostAddresses.Select(x => new Connection(x, "user", null, null));
            var connections = new ObservableCollectionEx<IConnection>(connectionSource);

            result.Connections.Returns(connections);

            result.LogIn(null, null, null).ReturnsForAnyArgs(x =>
            {
                var hostAddress = x.Arg<HostAddress>();

                if (hostAddress == InvalidHostAddress)
                {
                    throw new Exception("Invalid host address.");
                }

                var connection = new Connection(
                    hostAddress,
                    x.ArgAt<string>(1),
                    null,
                    null);
                connections.Add(connection);
                return connection;
            });

            result.LogOut(null).ReturnsForAnyArgs(x =>
            {
                var connection = connections.Single(y => y.HostAddress == x.Arg<HostAddress>());
                connections.Remove(connection);
                return Task.CompletedTask;
            });

            return result;
        }

        static IRepositoryHostFactory CreateRepositoryHostFactory()
        {
            var result = Substitute.For<IRepositoryHostFactory>();

            result.Create(null).ReturnsForAnyArgs(x =>
            {
                var host = Substitute.For<IRepositoryHost>();
                host.Address.Returns(x.Arg<IConnection>().HostAddress);
                return host;
            });

            return result;
        }
    }
}
