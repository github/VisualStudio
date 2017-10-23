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
            INotificationService notificationService = null,
            IConnectionManager connectionManager = null)
        {
            hosts = hosts ?? Substitutes.RepositoryHosts;
            service = service ?? Substitute.For<IRepositoryPublishService>();
            notificationService = notificationService ?? Substitute.For<INotificationService>();
            connectionManager = connectionManager ?? Substitutes.ConnectionManager;
            return new RepositoryPublishViewModel(hosts, service, notificationService, connectionManager,
                Substitute.For<IUsageTracker>());
        }

        public static void SetupConnections(IRepositoryHosts hosts, IConnectionManager cm,
            List<HostAddress> adds, List<IConnection> conns, List<IRepositoryHost> hsts,
            string uri)
        {
            var add = HostAddress.Create(new Uri(uri));
            var host = Substitute.For<IRepositoryHost>();
            var conn = Substitute.For<IConnection>();
            host.Address.Returns(add);
            conn.HostAddress.Returns(add);
            adds.Add(add);
            hsts.Add(host);
            conns.Add(conn);

            if (add.IsGitHubDotCom())
                hosts.GitHubHost.Returns(host);
            else
                hosts.EnterpriseHost.Returns(host);
            hosts.LookupHost(Arg.Is(add)).Returns(host);
        }

        public static IRepositoryPublishViewModel SetupConnectionsAndViewModel(
            IRepositoryHosts hosts = null,
            IRepositoryPublishService service = null,
            INotificationService notificationService = null,
            IConnectionManager cm = null,
            string uri = GitHubUrls.GitHub)
        {
            cm = cm ?? Substitutes.ConnectionManager;
            hosts = hosts ?? Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
            SetupConnections(hosts, cm, adds, conns, hsts, uri);
            hsts[0].ModelService.GetAccounts().Returns(Observable.Return(new List<IAccount>()));
            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));
            return GetViewModel(hosts, service, notificationService, cm);
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
        [InlineData(GitHubUrls.GitHub, "https://github.enterprise" )]
        [InlineData("https://github.enterprise", null)]
        [InlineData(GitHubUrls.GitHub, null)]
        public void ConnectionsMatchHosts(string arg1, string arg2)
        {
            var args = Helpers.GetArgs(arg1, arg2);

            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
            foreach (var uri in args)
                Helpers.SetupConnections(hosts, cm, adds, conns, hsts, uri);

            foreach(var host in hsts)
                host.ModelService.GetAccounts().Returns(Observable.Return(new List<IAccount>()));

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));

            var vm = Helpers.GetViewModel(hosts: hosts, connectionManager: cm);

            var connections = vm.Connections;

            Assert.Equal(args.Length, connections.Count);
            for (int i = 0; i < conns.Count; i++)
            {
                Assert.Same(hsts[i], hosts.LookupHost(conns[i].HostAddress));
            }
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
            var hosts = Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
            foreach (var uri in args)
                Helpers.SetupConnections(hosts, cm, adds, conns, hsts, uri);

            foreach (var host in hsts)
                host.ModelService.GetAccounts().Returns(Observable.Return(new List<IAccount>()));

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));
            var vm = Helpers.GetViewModel(hosts, connectionManager: cm);

            Assert.Same(adds.First(x => x.IsGitHubDotCom()), vm.SelectedConnection.HostAddress);
            Assert.Same(conns.First(x => x.HostAddress.IsGitHubDotCom()), vm.SelectedConnection);
            Assert.Same(hsts.First(x => x.Address.IsGitHubDotCom()), hosts.LookupHost(vm.SelectedConnection.HostAddress));
        }
    }

    public class TheAccountsProperty : TestBaseClass
    {
        [Fact]
        public void IsPopulatedByTheAccountsForTheSelectedHost()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
            Helpers.SetupConnections(hosts, cm, adds, conns, hsts, GitHubUrls.GitHub);
            Helpers.SetupConnections(hosts, cm, adds, conns, hsts, "https://github.enterprise");

            var gitHubAccounts = new List<IAccount> { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
            var enterpriseAccounts = new List<IAccount> { Substitute.For<IAccount>() };

            hsts.First(x => x.Address.IsGitHubDotCom()).ModelService.GetAccounts().Returns(Observable.Return(gitHubAccounts));
            hsts.First(x => !x.Address.IsGitHubDotCom()).ModelService.GetAccounts().Returns(Observable.Return(enterpriseAccounts));

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));
            var vm = Helpers.GetViewModel(hosts, connectionManager: cm);

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
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

            vm.RepositoryName = "this-is-bad";

            Assert.Equal(vm.RepositoryName, vm.SafeRepositoryName);
        }

        [Fact]
        public void IsConvertedWhenTheRepositoryNameIsNotSafe()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

            vm.RepositoryName = "this is bad";

            Assert.Equal("this-is-bad", vm.SafeRepositoryName);
        }

        [Fact]
        public void IsNullWhenRepositoryNameIsNull()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

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
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

            vm.RepositoryName = "";

            Assert.False(vm.RepositoryNameValidator.ValidationResult.IsValid);
            Assert.Equal("Please enter a repository name", vm.RepositoryNameValidator.ValidationResult.Message);
        }

        [Fact]
        public void IsFalseWhenAfterBeingTrue()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

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
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

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
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

            vm.RepositoryName = "this-is-bad";

            Assert.True(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
        }

        [Fact]
        public void IsFalseWhenRepoNameIsNotSafe()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

            vm.RepositoryName = "this is bad";

            Assert.False(vm.SafeRepositoryNameWarningValidator.ValidationResult.IsValid);
            Assert.Equal("Will be created as this-is-bad", vm.SafeRepositoryNameWarningValidator.ValidationResult.Message);
        }

        [Fact]
        public void ResetsSafeNameValidator()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, cm: cm);

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
            var hosts = Substitute.For<IRepositoryHosts>();
            var notificationService = Substitute.For<INotificationService>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, repositoryPublishService, notificationService, cm);
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
            var hosts = Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
            foreach (var uri in args)
                Helpers.SetupConnections(hosts, cm, adds, conns, hsts, uri);

            foreach (var host in hsts)
                host.ModelService.GetAccounts().Returns(Observable.Return(new List<IAccount>()));

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));

            var notificationService = Substitute.For<INotificationService>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.GetViewModel(hosts, repositoryPublishService, notificationService, cm);

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
            var hosts = Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
                Helpers.SetupConnections(hosts, cm, adds, conns, hsts, GitHubUrls.GitHub);

            var accounts = new List<IAccount> { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
            hsts[0].ModelService.GetAccounts().Returns(Observable.Return(accounts));

            cm.Connections.Returns(new ObservableCollectionEx<IConnection>(conns));

            var notificationService = Substitute.For<INotificationService>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.GetViewModel(hosts, repositoryPublishService, notificationService, cm);

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
