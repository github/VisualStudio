using System;
using System.Net;
using System.Reactive.Linq;
using GitHub.Authentication;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Octokit;
using ReactiveUI;
using NUnit.Framework;

public class LoginToGitHubViewModelTests
{
    //public class TheLoginCommand : TestBaseClass
    //{
    //    [Test]
    //    public void ShowsHelpfulTooltipWhenForbiddenResponseReceived()
    //    {
    //        var response = Substitute.For<IResponse>();
    //        response.StatusCode.Returns(HttpStatusCode.Forbidden);
    //        var repositoryHosts = Substitute.For<IRepositoryHosts>();
    //        repositoryHosts.LogIn(HostAddress.GitHubDotComHostAddress, Args.String, Args.String)
    //            .Returns(_ => Observable.Throw<AuthenticationResult>(new ForbiddenException(response)));
    //        var browser = Substitute.For<IVisualStudioBrowser>();
    //        var loginViewModel = new LoginToGitHubViewModel(repositoryHosts, browser);

    //        loginViewModel.Login.Execute(null);

    //        Assert.Equal("Make sure to use your password and not a Personal Access token to sign in.",
    //            loginViewModel.Error.ErrorMessage);
    //    }
    //}

    //public class TheSignupCommand : TestBaseClass
    //{
    //    [Test]
    //    public void LaunchesBrowserToSignUpPage()
    //    {
    //        var repositoryHosts = Substitute.For<IRepositoryHosts>();
    //        var gitHubHost = Substitute.For<IRepositoryHost>();
    //        gitHubHost.Address.Returns(HostAddress.GitHubDotComHostAddress);
    //        repositoryHosts.GitHubHost.Returns(gitHubHost);
    //        var browser = Substitute.For<IVisualStudioBrowser>();
    //        var loginViewModel = new LoginToGitHubViewModel(repositoryHosts, browser);

    //        loginViewModel.SignUp.Execute(null);

    //        browser.Received().OpenUrl(GitHubUrls.Plans);
    //    }
    //}

    //public class TheForgotPasswordCommand : TestBaseClass
    //{
    //    [Test]
    //    public void LaunchesBrowserToForgotPasswordPage()
    //    {
    //        var repositoryHosts = Substitute.For<IRepositoryHosts>();
    //        var gitHubHost = Substitute.For<IRepositoryHost>();
    //        gitHubHost.Address.Returns(HostAddress.GitHubDotComHostAddress);
    //        repositoryHosts.GitHubHost.Returns(gitHubHost);
    //        var browser = Substitute.For<IVisualStudioBrowser>();
    //        var loginViewModel = new LoginToGitHubViewModel(repositoryHosts, browser);

    //        loginViewModel.NavigateForgotPassword.Execute(null);

    //        browser.Received().OpenUrl(new Uri(HostAddress.GitHubDotComHostAddress.WebUri, GitHubUrls.ForgotPasswordPath));
    //    }
    //}
}
  