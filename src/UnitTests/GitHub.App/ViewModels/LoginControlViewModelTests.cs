using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Authentication;
using GitHub.Models;
using GitHub.ViewModels;
using NSubstitute;
using ReactiveUI;
using Xunit;

public class LoginControlViewModelTests
{
    public class TheAuthenticationResultsCommand : TestBaseClass
    {
        [Fact]
        public async Task SucessfulGitHubLoginSignalsDone()
        {
            var repositoryHosts = Substitute.For<IRepositoryHosts>();

            var gitHubLogin = Substitute.For<ILoginToGitHubViewModel>();
            var gitHubLoginCommand = ReactiveCommand.CreateAsyncObservable(_ =>
                Observable.Return(AuthenticationResult.Success));
            gitHubLogin.Login.Returns(gitHubLoginCommand);
            var enterpriseLogin = Substitute.For<ILoginToGitHubForEnterpriseViewModel>();

            var loginViewModel = new LoginControlViewModel(repositoryHosts, gitHubLogin, enterpriseLogin);
            var signalled = false;

            loginViewModel.Done.Subscribe(_ => signalled = true);
            await gitHubLoginCommand.ExecuteAsync();

            Assert.True(signalled);
        }

        [Fact]
        public async Task FailedGitHubLoginDoesNotSignalDone()
        {
            var repositoryHosts = Substitute.For<IRepositoryHosts>();

            var gitHubLogin = Substitute.For<ILoginToGitHubViewModel>();
            var gitHubLoginCommand = ReactiveCommand.CreateAsyncObservable(_ =>
                Observable.Return(AuthenticationResult.CredentialFailure));
            gitHubLogin.Login.Returns(gitHubLoginCommand);
            var enterpriseLogin = Substitute.For<ILoginToGitHubForEnterpriseViewModel>();

            var loginViewModel = new LoginControlViewModel(repositoryHosts, gitHubLogin, enterpriseLogin);
            var signalled = false;

            loginViewModel.Done.Subscribe(_ => signalled = true);
            await gitHubLoginCommand.ExecuteAsync();

            Assert.False(signalled);
        }

        [Fact]
        public async Task AllowsLoginFromEnterpriseAfterGitHubLoginHasFailed()
        {
            var repositoryHosts = Substitute.For<IRepositoryHosts>();

            var gitHubLogin = Substitute.For<ILoginToGitHubViewModel>();
            var gitHubLoginCommand = ReactiveCommand.CreateAsyncObservable(_ => 
                Observable.Return(AuthenticationResult.CredentialFailure));
            gitHubLogin.Login.Returns(gitHubLoginCommand);

            var enterpriseLogin = Substitute.For<ILoginToGitHubForEnterpriseViewModel>();
            var enterpriseLoginCommand = ReactiveCommand.CreateAsyncObservable(_ =>
                Observable.Return(AuthenticationResult.Success));
            enterpriseLogin.Login.Returns(enterpriseLoginCommand);

            var loginViewModel = new LoginControlViewModel(repositoryHosts, gitHubLogin, enterpriseLogin);
            var success = false;

            loginViewModel.AuthenticationResults
                .Where(x => x == AuthenticationResult.Success)
                .Subscribe(_ => success = true);

            await gitHubLoginCommand.ExecuteAsync();
            await enterpriseLoginCommand.ExecuteAsync();

            Assert.True(success);
        }
    }
}
  