using System;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GitHub.Authentication;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Octokit;
using ReactiveUI;
using Xunit;

public class LoginControlViewModelTests
{
    public class TheAuthenticationResultsCommand : TestBaseClass
    {
        [Fact]
        public async void AllowsLoginFromEnterpriseAfterGitHubLoginHasFailed()
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
  