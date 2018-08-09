using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.Dialog
{
    public class GitHubDialogWindowViewModelTests
    {
        public class TheStartMethod : TestBaseClass
        {
            [Test]
            public void SetsContent()
            {
                var target = CreateTarget();
                var content = Substitute.For<IDialogContentViewModel>();

                target.Start(content);

                Assert.That(content, Is.SameAs(target.Content));
            }

            [Test]
            public void SignalsCloseWhenContentRaisesClosed()
            {
                var target = CreateTarget();
                var content = Substitute.For<IDialogContentViewModel>();
                var closed = new Subject<object>();
                var signalled = false;

                content.Done.Returns(closed);
                target.Done.Subscribe(_ => signalled = true);
                target.Start(content);
                closed.OnNext(null);

                Assert.True(signalled);
            }
        }

        public class TheStartWithConnectionMethod
        {
            [Test]
            public async Task ShowsLoginDialogWhenNoConnectionsAvailableAsync()
            {
                var target = CreateTarget();
                var content = Substitute.For<ITestViewModel>();

                await target.StartWithConnection(content);

                Assert.That(target.Content, Is.InstanceOf<ILoginViewModel>());
            }

            [Test]
            public async Task ShowsLoginDialogWhenSpecifiedHostAddressIsNotAvailableAsync()
            {
                var enterpriseHostAddress = HostAddress.Create("github-enterprise.com");
                var connectionManager = CreateConnectionManager(enterpriseHostAddress);
                var target = CreateTarget(connectionManager);
                var content = Substitute.For<ITestViewModel>();

                await target.StartWithConnection(content, HostAddress.GitHubDotComHostAddress);

                Assert.That(target.Content, Is.InstanceOf<ILoginViewModel>());
            }

            [Test]
            public async Task ShowsContentWhenSpecifiedHostAddressIsAvailableAsync()
            {
                var enterpriseHostAddress = HostAddress.Create("github-enterprise.com");
                var gitHubHostAddress = HostAddress.GitHubDotComHostAddress;
                var connectionManager = CreateConnectionManager(enterpriseHostAddress, gitHubHostAddress);
                var target = CreateTarget(connectionManager);
                var content = Substitute.For<ITestViewModel>();

                await target.StartWithConnection(content, gitHubHostAddress);

                Assert.That(content, Is.SameAs(target.Content));
                await content.Received(1).InitializeAsync(connectionManager.Connections[1]);
            }

            [Test]
            public async Task ShowsContentWhenConnectionAvailableAsync()
            {
                var connectionManager = CreateConnectionManager(HostAddress.GitHubDotComHostAddress);
                var target = CreateTarget(connectionManager);
                var content = Substitute.For<ITestViewModel>();

                await target.StartWithConnection(content);

                Assert.That(content, Is.SameAs(target.Content));
                await content.Received(1).InitializeAsync(connectionManager.Connections[0]);
            }

            [Test]
            public async Task ShowsContentWhenLoggedInAsync()
            {
                var target = CreateTarget();
                var content = Substitute.For<ITestViewModel>();

                await target.StartWithConnection(content);

                var login = (ILoginViewModel)target.Content;
                var connection = Substitute.For<IConnection>();
                ((ISubject<object>)login.Done).OnNext(connection);

                Assert.That(content, Is.SameAs(target.Content));
                await content.Received(1).InitializeAsync(connection);
            }

            [Test]
            public async Task ClosesDialogWhenLoginReturnsNullConnectionAsync()
            {
                var target = CreateTarget();
                var content = Substitute.For<ITestViewModel>();
                var closed = false;

                target.Done.Subscribe(_ => closed = true);
                await target.StartWithConnection(content);

                var login = (ILoginViewModel)target.Content;
                ((ISubject<object>)login.Done).OnNext(null);

                Assert.True(closed);
            }
        }

        static IConnectionManager CreateConnectionManager(params HostAddress[] hostAddresses)
        {
            var connections = new ObservableCollectionEx<IConnection>();

            foreach (var hostAddress in hostAddresses)
            {
                var connection = Substitute.For<IConnection>();
                connection.HostAddress.Returns(hostAddress);
                connection.IsLoggedIn.Returns(true);
                connections.Add(connection);
            }

            var result = Substitute.For<IConnectionManager>();
            result.Connections.Returns(connections);
            result.GetLoadedConnections().Returns(connections);
            return result;
        }

        static GitHubDialogWindowViewModel CreateTarget(IConnectionManager connectionManager = null)
        {
            var login = Substitute.For<ILoginViewModel>();
            login.Done.Returns(new Subject<object>());

            var factory = Substitute.For<IViewViewModelFactory>();
            factory.CreateViewModel<ILoginViewModel>().Returns(login);

            connectionManager = connectionManager ?? Substitute.For<IConnectionManager>();

            return new GitHubDialogWindowViewModel(
                factory,
                new Lazy<IConnectionManager>(() => connectionManager));
        }

        public interface ITestViewModel : IDialogContentViewModel, IConnectionInitializedViewModel
        {
        }
    }
}
