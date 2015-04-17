using System.Reactive.Linq;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using ReactiveUI;
using Xunit;
using UnitTests;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using System.Reactive;
using System.Threading.Tasks;
using System;

public class RepositoryPublishViewModelTests
{
    public static class Helpers
    {
        public static IRepositoryPublishViewModel GetViewModel(IRepositoryPublishService service = null)
        {
            return GetViewModel(null, service, null);
        }

        public static IRepositoryPublishViewModel GetViewModel(
            IRepositoryHosts hosts = null,
            IRepositoryPublishService service = null,
            IVSServices vsServices = null)
        {
            hosts = hosts ?? Substitutes.RepositoryHosts;
            service = service ?? Substitute.For<IRepositoryPublishService>();
            vsServices = vsServices ?? Substitute.For<IVSServices>();
            return new RepositoryPublishViewModel(hosts, service, vsServices);
        }
    }

    public class TheRepositoryHostsProperty
    {
        [Theory]
        [InlineData(true, true, 2)]
        [InlineData(true, false, 1)]
        [InlineData(false, true, 1)]
        public void IncludesLoggedInRepositories(bool gitHubLoggedIn, bool enterpriseLoggedIn, int expectedCount)
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(gitHubLoggedIn);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var enterpriseHost = Substitute.For<IRepositoryHost>();
            enterpriseHost.IsLoggedIn.Returns(enterpriseLoggedIn);
            enterpriseHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            enterpriseHost.Title.Returns("ghe.io");
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            hosts.EnterpriseHost.Returns(enterpriseHost);
            var vm = Helpers.GetViewModel(hosts);

            var repositoryHosts = vm.RepositoryHosts;

            Assert.Equal(expectedCount, repositoryHosts.Count);
        }
    }

    public class TheSelectedHostProperty
    {
        [Fact]
        public void DefaultsToGitHub()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);

            Assert.Same(gitHubHost, vm.SelectedHost);
        }
    }

    public class TheAccountsProperty
    {
        [Fact]
        public void IsPopulatedByTheAccountsForTheSelectedHost()
        {
            var gitHubAccounts = new ReactiveList<IAccount> { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
            var enterpriseAccounts = new ReactiveList<IAccount> { Substitute.For<IAccount>() };
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(gitHubAccounts));
            var enterpriseHost = Substitute.For<IRepositoryHost>();
            enterpriseHost.IsLoggedIn.Returns(true);
            enterpriseHost.ModelService.GetAccounts().Returns(Observable.Return(enterpriseAccounts));
            enterpriseHost.Title.Returns("ghe.io");
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            hosts.EnterpriseHost.Returns(enterpriseHost);
            var vm = Helpers.GetViewModel(hosts);

            Assert.Equal(2, vm.Accounts.Count);
            Assert.Same(gitHubAccounts[0], vm.SelectedAccount);

            vm.SelectedHost = enterpriseHost;

            Assert.Equal(1, vm.Accounts.Count);
            Assert.Same(enterpriseAccounts[0], vm.SelectedAccount);
        }
    }

    public class TheSafeRepositoryNameProperty
    {
        [Fact]
        public void IsTheSameAsTheRepositoryNameWhenTheInputIsSafe()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);

            vm.RepositoryName = "this-is-bad";

            Assert.Equal(vm.RepositoryName, vm.SafeRepositoryName);
        }

        [Fact]
        public void IsConvertedWhenTheRepositoryNameIsNotSafe()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);

            vm.RepositoryName = "this is bad";

            Assert.Equal("this-is-bad", vm.SafeRepositoryName);
        }

        [Fact]
        public void IsNullWhenRepositoryNameIsNull()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);
            Assert.Null(vm.SafeRepositoryName);
            vm.RepositoryName = "not-null";
            vm.RepositoryName = null;

            Assert.Null(vm.SafeRepositoryName);
        }
    }

    public class TheRepositoryNameValidatorProperty
    {
        [Fact]
        public void IsFalseWhenRepoNameEmpty()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenAfterBeingTrue()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);
            vm.RepositoryName = "repo";

            Assert.True(vm.PublishRepository.CanExecute(null));
            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsTrueWhenRepositoryNameIsValid()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);

            vm.RepositoryName = "thisisfine";

            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);
        }
    }

    public class TheSafeRepositoryNameWarningValidatorProperty
    {
        [Fact]
        public void IsTrueWhenRepoNameIsSafe()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);

            vm.RepositoryName = "this-is-bad";

            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
        }

        [Fact]
        public void IsFalseWhenRepoNameIsNotSafe()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vm = Helpers.GetViewModel(hosts);

            vm.RepositoryName = "this is bad";

            Assert.False(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
            Assert.Equal("Will be created as this-is-bad", vm.SafeRepositoryNameWarningValidator.ValidationResult.Message);
        }

        [Fact]
        public void DisplaysWarningWhenRepoNameNotSafeAndClearsItWhenSafeAgain()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vsServices = Substitute.For<IVSServices>();
            var vm = Helpers.GetViewModel(hosts, vsServices: vsServices);
            vsServices.DidNotReceive().ShowWarning(Args.String);

            vm.RepositoryName = "this is bad";
            Assert.Equal("this-is-bad", vm.SafeRepositoryName);

            vsServices.Received().ShowWarning("Will be created as this-is-bad");
            vsServices.DidNotReceive().ClearNotifications();

            vm.RepositoryName = "this";

            vsServices.Received().ClearNotifications();
        }
    }

    public class ThePublishRepositoryCommand
    {
        [Fact]
        public async Task DisplaysSuccessMessageWhenCompletedWithoutError()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vsServices = Substitute.For<IVSServices>();
            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Return(new Octokit.Repository()));
            var vm = Helpers.GetViewModel(hosts, service: repositoryPublishService, vsServices: vsServices);
            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(Unit.Default));

            vsServices.Received().ShowMessage("Repository published successfully.");
            vsServices.DidNotReceive().ShowError(Args.String);
        }

        [Fact]
        public async Task DisplaysRepositoryExistsErrorWithVisualStudioNotifications()
        {
            var gitHubHost = Substitute.For<IRepositoryHost>();
            gitHubHost.IsLoggedIn.Returns(true);
            gitHubHost.Title.Returns("GitHub");
            gitHubHost.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.GitHubHost.Returns(gitHubHost);
            var vsServices = Substitute.For<IVSServices>();
            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.GetViewModel(hosts, service: repositoryPublishService, vsServices: vsServices);
            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(Unit.Default));

            vsServices.DidNotReceive().ShowMessage(Args.String);
            vsServices.Received().ShowError("There is already a repository named 'repo-name' for the current account.");
        }
    }
}
