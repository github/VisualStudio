using System.Reactive.Linq;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Xunit;
using UnitTests;
using System.Threading.Tasks;
using System;
using GitHub.Primitives;
using GitHub.Info;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GitHub.Extensions;
using GitHub.Factories;

public class RepositoryPublishViewModelTests
{
    public static class Helpers
    {
        public static IRepositoryPublishViewModel GetViewModel(IRepositoryPublishService service = null)
        {
            return GetViewModel(service);
        }

        public static IRepositoryPublishViewModel GetViewModel(
            IRepositoryPublishService service = null,
            INotificationService notificationService = null,
            IConnectionManager connectionManager = null,
            IModelServiceFactory factory = null)
        {
            service = service ?? Substitute.For<IRepositoryPublishService>();
            notificationService = notificationService ?? Substitute.For<INotificationService>();
            connectionManager = connectionManager ?? Substitutes.ConnectionManager;
            factory = factory ?? Substitute.For<IModelServiceFactory>();

            return new RepositoryPublishViewModel(service, notificationService, connectionManager,
                factory, Substitute.For<IUsageTracker>());
        }

        public static void SetupConnections(List<HostAddress> adds, List<IConnection> conns, string uri)
        {
            var add = HostAddress.Create(new Uri(uri));
            var conn = Substitute.For<IConnection>();
            conn.HostAddress.Returns(add);
            adds.Add(add);
            conns.Add(conn);
        }

        public static IRepositoryPublishViewModel SetupConnectionsAndViewModel(
            IRepositoryPublishService service = null,
            INotificationService notificationService = null,
            IConnectionManager cm = null,
            string uri = GitHubUrls.GitHub)
        {
            cm = cm ?? Substitutes.ConnectionManager;
            var adds = new List<HostAddress>();
            var conns = new List<IConnection>();
            SetupConnections(adds, conns, uri);
            //hsts[0].ModelService.GetAccounts().Returns(Observable.Return(new List<IAccount>()));
            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));
            return GetViewModel(service, notificationService, cm);
        }

        public static string[] GetArgs(params string[] args)
        {
            var ret = new List<string>();
            foreach (var arg in args)
                if (arg != null)
                    ret.Add(arg);
            return ret.ToArray();
        }

    }

    public class TheConnectionsProperty : TestBaseClass
    {
        [Theory]
        [InlineData(GitHubUrls.GitHub, "https://github.enterprise")]
        [InlineData("https://github.enterprise", null)]
        [InlineData(GitHubUrls.GitHub, null)]
        public void ConnectionsMatchConnectionManager(string arg1, string arg2)
        {
            var args = Helpers.GetArgs(arg1, arg2);

            var cm = Substitutes.ConnectionManager;
            var adds = new List<HostAddress>();
            var conns = new List<IConnection>();
            foreach (var uri in args)
                Helpers.SetupConnections(adds, conns, uri);

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));

            var vm = Helpers.GetViewModel(connectionManager: cm);

            Assert.Equal(conns, vm.Connections);
        }
    }

    public class TheSelectedConnectionProperty : TestBaseClass
    {
        [Theory]
        [InlineData(GitHubUrls.GitHub, "https://github.enterprise")]
        [InlineData("https://github.enterprise", GitHubUrls.GitHub)]
        public void DefaultsToGitHub(string arg1, string arg2)
        {
            var args = Helpers.GetArgs(arg1, arg2);

            var cm = Substitutes.ConnectionManager;
            var adds = new List<HostAddress>();
            var conns = new List<IConnection>();
            foreach (var uri in args)
                Helpers.SetupConnections(adds, conns, uri);

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));
            var vm = Helpers.GetViewModel(connectionManager: cm);

            Assert.Same(adds.First(x => x.IsGitHubDotCom()), vm.SelectedConnection.HostAddress);
            Assert.Same(conns.First(x => x.HostAddress.IsGitHubDotCom()), vm.SelectedConnection);
        }
    }

    public class TheAccountsProperty : TestBaseClass
    {
        [Fact]
        public void IsPopulatedByTheAccountsForTheSelectedHost()
        {
            var cm = Substitutes.ConnectionManager;
            var adds = new List<HostAddress>();
            var conns = new List<IConnection>();
            Helpers.SetupConnections(adds, conns, GitHubUrls.GitHub);
            Helpers.SetupConnections(adds, conns, "https://github.enterprise");

            var gitHubAccounts = new List<IAccount> { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
            var enterpriseAccounts = new List<IAccount> { Substitute.For<IAccount>() };

            var gitHubModelService = Substitute.For<IModelService>();
            var enterpriseModelService = Substitute.For<IModelService>();
            gitHubModelService.GetAccounts().Returns(Observable.Return(gitHubAccounts));
            enterpriseModelService.GetAccounts().Returns(Observable.Return(enterpriseAccounts));

            var factory = Substitute.For<IModelServiceFactory>();
            factory.CreateAsync(conns[0]).Returns(gitHubModelService);
            factory.CreateAsync(conns[1]).Returns(enterpriseModelService);

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));
            var vm = Helpers.GetViewModel(connectionManager: cm, factory: factory);

            Assert.Equal(2, vm.Accounts.Count);
            Assert.Same(gitHubAccounts[0], vm.SelectedAccount);

            vm.SelectedConnection = conns.First(x => !x.HostAddress.IsGitHubDotCom());

            Assert.Equal(1, vm.Accounts.Count);
            Assert.Same(enterpriseAccounts[0], vm.SelectedAccount);
        }
    }

    public class TheSafeRepositoryNameProperty : TestBaseClass
    {
        [Fact]
        public void IsTheSameAsTheRepositoryNameWhenTheInputIsSafe()
        {
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

            vm.RepositoryName = "this-is-bad";

            Assert.Equal(vm.RepositoryName, vm.SafeRepositoryName);
        }

        [Fact]
        public void IsConvertedWhenTheRepositoryNameIsNotSafe()
        {
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

            vm.RepositoryName = "this is bad";

            Assert.Equal("this-is-bad", vm.SafeRepositoryName);
        }

        [Fact]
        public void IsNullWhenRepositoryNameIsNull()
        {
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

            Assert.Null(vm.SafeRepositoryName);
            vm.RepositoryName = "not-null";
            vm.RepositoryName = null;

            Assert.Null(vm.SafeRepositoryName);
        }
    }

    public class TheRepositoryNameValidatorProperty : TestBaseClass
    {
        [Fact]
        public void IsFalseWhenRepoNameEmpty()
        {
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenAfterBeingTrue()
        {
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

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
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

            vm.RepositoryName = "thisisfine";

            Assert.True(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Empty(vm.RepositoryNameValidator.ValidationResult.Message);
        }
    }

    public class TheSafeRepositoryNameWarningValidatorProperty : TestBaseClass
    {
        [Fact]
        public void IsTrueWhenRepoNameIsSafe()
        {
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

            vm.RepositoryName = "this-is-bad";

            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
        }

        [Fact]
        public void IsFalseWhenRepoNameIsNotSafe()
        {
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

            vm.RepositoryName = "this is bad";

            Assert.False(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
            Assert.Equal("Will be created as this-is-bad", vm.SafeRepositoryNameWarningValidator.ValidationResult.Message);
        }

        [Fact]
        public void ResetsSafeNameValidator()
        {
            var cm = Substitutes.ConnectionManager;
            var vm = Helpers.SetupConnectionsAndViewModel(cm: cm);

            vm.RepositoryName = "this";
            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);

            vm.RepositoryName = "this is bad";
            Assert.Equal("this-is-bad", vm.SafeRepositoryName);
            Assert.False(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);

            vm.RepositoryName = "this";

            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
        }
    }

    public class ThePublishRepositoryCommand : TestBaseClass
    {
        [Fact]
        public async Task RepositoryExistsCallsNotificationServiceWithError()
        {
            var cm = Substitutes.ConnectionManager;
            var notificationService = Substitute.For<INotificationService>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.SetupConnectionsAndViewModel(repositoryPublishService, notificationService, cm);
            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(ProgressState.Fail));

            Assert.NotNull(vm.SafeRepositoryNameWarningValidator.ValidationResult.Message);
            notificationService.DidNotReceive().ShowMessage(Args.String);
            notificationService.Received().ShowError("There is already a repository named 'repo-name' for the current account.");
        }

        [Fact]
        public async Task ResetsWhenSwitchingHosts()
        {
            var args = Helpers.GetArgs(GitHubUrls.GitHub, "https://github.enterprise");

            var cm = Substitutes.ConnectionManager;
            var adds = new List<HostAddress>();
            var conns = new List<IConnection>();
            foreach (var uri in args)
                Helpers.SetupConnections(adds, conns, uri);

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));

            var notificationService = Substitute.For<INotificationService>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.GetViewModel(repositoryPublishService, notificationService, cm);

            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(ProgressState.Fail));

            Assert.Equal("repo-name", vm.RepositoryName);
            notificationService.Received().ShowError("There is already a repository named 'repo-name' for the current account.");

            var wasCalled = false;
            vm.SafeRepositoryNameWarningValidator.PropertyChanged += (s, e) => wasCalled = true;

            vm.SelectedConnection = conns.First(x => x != vm.SelectedConnection);

            Assert.True(wasCalled);
        }

        [Fact]
        public async Task ResetsWhenSwitchingAccounts()
        {
            var cm = Substitutes.ConnectionManager;
            var adds = new List<HostAddress>();
            var conns = new List<IConnection>();
            Helpers.SetupConnections(adds, conns, GitHubUrls.GitHub);

            var accounts = new List<IAccount> { Substitute.For<IAccount>(), Substitute.For<IAccount>() };

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));

            var notificationService = Substitute.For<INotificationService>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.GetViewModel(repositoryPublishService, notificationService, cm);

            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(ProgressState.Fail));

            Assert.Equal("repo-name", vm.RepositoryName);
            notificationService.Received().ShowError("There is already a repository named 'repo-name' for the current account.");

            var wasCalled = false;
            vm.SafeRepositoryNameWarningValidator.PropertyChanged += (s, e) => wasCalled = true;

            vm.SelectedAccount = accounts[1];

            Assert.True(wasCalled);
        }
    }
}
