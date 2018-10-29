using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels.Dialog;
using NSubstitute;
using ReactiveUI;
using NUnit.Framework;
using GitHub.ViewModels;
using ReactiveUI.Testing;
using System.Reactive.Concurrency;

public class LoginCredentialsViewModelTests
{
    public class TheDoneSignal : TestBaseClass
    {
        [Test]
        public async Task SucessfulGitHubLoginSignalsDoneAsync()
        {
            var connectionManager = Substitute.For<IConnectionManager>();
            var connection = Substitute.For<IConnection>();

            var gitHubLogin = CreateLoginToHostViewModel<ILoginToGitHubViewModel>(connection);
            var enterpriseLogin = CreateLoginToHostViewModel<ILoginToGitHubForEnterpriseViewModel>();
            var loginViewModel = new LoginCredentialsViewModel(connectionManager, gitHubLogin, enterpriseLogin);
            var signalled = false;

            loginViewModel.Done.Subscribe(_ => signalled = true);
            await gitHubLogin.Login.Execute();

            Assert.True(signalled);
        }

        [Test]
        public async Task FailedGitHubLoginDoesNotSignalDoneAsync()
        {
            var connectionManager = Substitute.For<IConnectionManager>();

            var gitHubLogin = CreateLoginToHostViewModel<ILoginToGitHubViewModel>();
            var enterpriseLogin = CreateLoginToHostViewModel<ILoginToGitHubForEnterpriseViewModel>();
            var loginViewModel = new LoginCredentialsViewModel(connectionManager, gitHubLogin, enterpriseLogin);
            var signalled = false;

            loginViewModel.Done.Subscribe(_ => signalled = true);
            await gitHubLogin.Login.Execute();

            Assert.False(signalled);
        }

        [Test]
        public async Task AllowsLoginFromEnterpriseAfterGitHubLoginHasFailedAsync()
        {
            var connectionManager = Substitute.For<IConnectionManager>();
            var connection = Substitute.For<IConnection>();

            var gitHubLogin = CreateLoginToHostViewModel<ILoginToGitHubViewModel>();
            var enterpriseLogin = CreateLoginToHostViewModel<ILoginToGitHubForEnterpriseViewModel>(connection);
            var loginViewModel = new LoginCredentialsViewModel(connectionManager, gitHubLogin, enterpriseLogin);
            var success = false;

            loginViewModel.Done
                .OfType<IConnection>()
                .Where(x => x != null)
                .Subscribe(_ => success = true);

            await gitHubLogin.Login.Execute();
            await enterpriseLogin.Login.Execute();

            Assert.True(success);
        }
    }

    public class TheLoginModeProperty : TestBaseClass
    {
        [Test]
        public void LoginModeTracksAvailableConnections()
        {
            var connectionManager = Substitute.For<IConnectionManager>();
            var connections = new ObservableCollectionEx<IConnection>();
            var gitHubLogin = Substitute.For<ILoginToGitHubViewModel>();
            var enterpriseLogin = Substitute.For<ILoginToGitHubForEnterpriseViewModel>();
            var gitHubConnection = Substitute.For<IConnection>();
            var enterpriseConnection = Substitute.For<IConnection>();

            connectionManager.Connections.Returns(connections);
            gitHubConnection.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
            enterpriseConnection.HostAddress.Returns(HostAddress.Create("https://enterprise.url"));
            gitHubConnection.IsLoggedIn.Returns(true);
            enterpriseConnection.IsLoggedIn.Returns(true);

            var loginViewModel = new LoginCredentialsViewModel(connectionManager, gitHubLogin, enterpriseLogin);

            Assert.That(LoginMode.DotComOrEnterprise, Is.EqualTo(loginViewModel.LoginMode));

            connections.Add(enterpriseConnection);
            Assert.That(LoginMode.DotComOnly, Is.EqualTo(loginViewModel.LoginMode));

            connections.Add(gitHubConnection);
            Assert.That(LoginMode.None, Is.EqualTo(loginViewModel.LoginMode));

            connections.RemoveAt(0);
            Assert.That(LoginMode.EnterpriseOnly, Is.EqualTo(loginViewModel.LoginMode));
        }
    }

    static T CreateLoginToHostViewModel<T>(IConnection login = null, IConnection oauthLogin = null)
        where T : class, ILoginToHostViewModel
    {
        var result = Substitute.For<T>();
        result.Login.Returns(ReactiveCommand.Create(() => login));
        result.LoginViaOAuth.Returns(ReactiveCommand.Create(() => oauthLogin));
        return result;
    }
}
