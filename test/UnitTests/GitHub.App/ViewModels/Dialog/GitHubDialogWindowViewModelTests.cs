using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog;
using NSubstitute;
using Xunit;

namespace UnitTests.GitHub.App.ViewModels.Dialog
{
    public class GitHubDialogWindowViewModelTests
    {
        public class TheStartMethod : TestBaseClass
        {
            [Fact]
            public void SetsContent()
            {
                var target = CreateTarget();
                var content = Substitute.For<IDialogContentViewModel>();

                target.Start(content);

                Assert.Same(content, target.Content);
            }

            [Fact]
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
            [Fact]
            public async Task ShowsLoginDialogWhenNoConnectionsAvailable()
            {
                var target = CreateTarget();
                var content = Substitute.For<ITestViewModel>();

                await target.StartWithConnection(content);

                Assert.IsAssignableFrom<ILoginViewModel>(target.Content);
            }

            [Fact]
            public async Task ShowsContentWhenConnectionAvailable()
            {
                var connectionManager = CreateConnectionManager(1);
                var target = CreateTarget(connectionManager);
                var content = Substitute.For<ITestViewModel>();

                await target.StartWithConnection(content);

                Assert.Same(content, target.Content);
                await content.Received(1).InitializeAsync(connectionManager.Connections[0]);
            }

            [Fact]
            public async Task ShowsContentWhenLoggedIn()
            {
                var target = CreateTarget();
                var content = Substitute.For<ITestViewModel>();

                await target.StartWithConnection(content);

                var login = (ILoginViewModel)target.Content;
                var connection = Substitute.For<IConnection>();
                ((ISubject<object>)login.Done).OnNext(connection);

                Assert.Same(content, target.Content);
                await content.Received(1).InitializeAsync(connection);
            }

            [Fact]
            public async Task ClosesDialogWhenLoginReturnsNullConnection()
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

        static IConnectionManager CreateConnectionManager(int numberOfConnections)
        {
            var connections = new ObservableCollectionEx<IConnection>();

            for (var i = 0; i < numberOfConnections; ++i)
            {
                var connection = Substitute.For<IConnection>();
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

            var serviceProvider = Substitute.For<IGitHubServiceProvider>();
            serviceProvider.GetService<ILoginViewModel>().Returns(login);

            connectionManager = connectionManager ?? Substitute.For<IConnectionManager>();

            return new GitHubDialogWindowViewModel(
                serviceProvider,
                new Lazy<IConnectionManager>(() => connectionManager));
        }

        public interface ITestViewModel : IDialogContentViewModel, IConnectionInitializedViewModel
        {
        }
    }
}
