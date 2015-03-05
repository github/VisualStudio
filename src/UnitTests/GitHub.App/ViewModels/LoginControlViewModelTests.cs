//using System;
//using GitHub.Info;
//using GitHub.Models;
//using GitHub.Primitives;
//using GitHub.Services;
//using GitHub.ViewModels;
//using NSubstitute;
//using Xunit;

//public class LoginControlViewModelTests
//{
//    public class TheSignupCommand
//    {
//        [Fact]
//        public void LaunchesBrowserToSignUpPage()
//        {
//            var serviceProvider = Substitute.For<IServiceProvider>();
//            var repositoryHosts = Substitute.For<IRepositoryHosts>();
//            var browser = Substitute.For<IBrowser>();
//            var enterpriseProbe = LazySubstitute.For<IEnterpriseProbe>();
//            var loginViewModel = new LoginControlViewModel(serviceProvider, repositoryHosts, browser, enterpriseProbe);

//            loginViewModel.SignUpCommand.Execute(null);

//            browser.Received().OpenUrl(GitHubUrls.Plans);
//        }
//    }

//    public class TheForgotPasswordCommand
//    {
//        [Fact]
//        public void LaunchesBrowserToForgotPasswordPage()
//        {
//            var serviceProvider = Substitute.For<IServiceProvider>();
//            var repositoryHosts = Substitute.For<IRepositoryHosts>();
//            var browser = Substitute.For<IBrowser>();
//            var enterpriseProbe = LazySubstitute.For<IEnterpriseProbe>();
//            var loginViewModel = new LoginControlViewModel(serviceProvider, repositoryHosts, browser, enterpriseProbe);

//            loginViewModel.ForgotPasswordCommand.Execute(null);

//            browser.Received().OpenUrl(new Uri(HostAddress.GitHubDotComHostAddress.WebUri, GitHubUrls.ForgotPasswordPath));
//        }
//    }
//}
