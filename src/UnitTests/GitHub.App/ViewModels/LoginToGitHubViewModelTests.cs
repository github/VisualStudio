using System;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Xunit;

public class LoginToGitHubViewModelTests
{
    public class TheSignupCommand
    {
        [Fact]
        public void LaunchesBrowserToSignUpPage()
        {
            var repositoryHosts = Substitute.For<IRepositoryHosts>();
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.Address.Returns(HostAddress.GitHubDotComHostAddress);
            repositoryHosts.GitHubHost.Returns(gitHubHost);
            var browser = Substitute.For<IVisualStudioBrowser>();
            var loginViewModel = new LoginToGitHubViewModel(repositoryHosts, browser);

            loginViewModel.SignUp.Execute(null);

            browser.Received().OpenUrl(GitHubUrls.Plans);
        }
    }

    public class TheForgotPasswordCommand
    {
        [Fact(Skip = "It's flipflopping with xunit")]
        public void LaunchesBrowserToForgotPasswordPage()
        {
            var repositoryHosts = Substitute.For<IRepositoryHosts>();
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.Address.Returns(HostAddress.GitHubDotComHostAddress);
            repositoryHosts.GitHubHost.Returns(gitHubHost);
            var browser = Substitute.For<IVisualStudioBrowser>();
            var loginViewModel = new LoginToGitHubViewModel(repositoryHosts, browser);

            loginViewModel.ForgotPassword.Execute(null);

            browser.Received().OpenUrl(new Uri(HostAddress.GitHubDotComHostAddress.WebUri, GitHubUrls.ForgotPasswordPath));
        }
    }
}
  