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
using GitHub.Primitives;
using GitHub.Info;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            IVSServices vsServices = null,
            IConnectionManager connectionManager = null)
        {
            hosts = hosts ?? Substitutes.RepositoryHosts;
            service = service ?? Substitute.For<IRepositoryPublishService>();
            vsServices = vsServices ?? Substitute.For<IVSServices>();
            connectionManager = connectionManager ?? Substitutes.ConnectionManager;
            return new RepositoryPublishViewModel(hosts, service, vsServices, connectionManager);
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
            IVSServices vs = null,
            IConnectionManager cm = null,
            string uri = GitHubUrls.GitHub)
        {
            cm = cm ?? Substitutes.ConnectionManager;
            hosts = hosts ?? Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
            SetupConnections(hosts, cm, adds, conns, hsts, uri);
            hsts[0].ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));
            cm.Connections.Returns(new ObservableCollection<IConnection>(conns));
            return GetViewModel(hosts, service, vs, cm);
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
        [InlineData(GitHubUrls.GitHub, "https://ghe.io" )]
        [InlineData("https://ghe.io", null)]
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
                host.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));

            cm.Connections.Returns(new ObservableCollection<IConnection>(conns));

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
        [InlineData(GitHubUrls.GitHub, "https://ghe.io")]
        [InlineData("https://ghe.io", GitHubUrls.GitHub)]
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
                host.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));

            cm.Connections.Returns(new ObservableCollection<IConnection>(conns));
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
            Helpers.SetupConnections(hosts, cm, adds, conns, hsts, "https://ghe.io");

            var gitHubAccounts = new ReactiveList<IAccount> { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
            var enterpriseAccounts = new ReactiveList<IAccount> { Substitute.For<IAccount>() };

            hsts.First(x => x.Address.IsGitHubDotCom()).ModelService.GetAccounts().Returns(Observable.Return(gitHubAccounts));
            hsts.First(x => !x.Address.IsGitHubDotCom()).ModelService.GetAccounts().Returns(Observable.Return(enterpriseAccounts));

            cm.Connections.Returns(new ObservableCollection<IConnection>(conns));
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
        public void DisplaysWarningWhenRepoNameNotSafeAndClearsItWhenSafeAgain()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var vsServices = Substitute.For<IVSServices>();
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, vs: vsServices, cm: cm);

            vsServices.DidNotReceive().ShowWarning(Args.String);

            vm.RepositoryName = "this is bad";
            Assert.Equal("this-is-bad", vm.SafeRepositoryName);

            vsServices.Received().ShowWarning("Will be created as this-is-bad");
            vsServices.DidNotReceive().ClearNotifications();

            vm.RepositoryName = "this";

            vsServices.Received().ClearNotifications();
        }
    }

    public class ThePublishRepositoryCommand : TestBaseClass
    {
        [Fact]
        public async Task DisplaysSuccessMessageWhenCompletedWithoutError()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var vsServices = Substitute.For<IVSServices>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Return(new Octokit.Repository()));

            var vm = Helpers.SetupConnectionsAndViewModel(hosts, repositoryPublishService, vsServices, cm);

            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(ProgressState.Success));

            vsServices.Received().ShowMessage("Repository published successfully.");
            vsServices.DidNotReceive().ShowError(Args.String);
        }

        [Fact]
        public async Task DisplaysRepositoryExistsErrorWithVisualStudioNotifications()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var vsServices = Substitute.For<IVSServices>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.SetupConnectionsAndViewModel(hosts, repositoryPublishService, vsServices, cm);
            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(ProgressState.Fail));

            vsServices.DidNotReceive().ShowMessage(Args.String);
            vsServices.Received().ShowError("There is already a repository named 'repo-name' for the current account.");
        }

        [Fact]
        public async Task ClearsErrorsWhenSwitchingHosts()
        {
            var args = Helpers.GetArgs(GitHubUrls.GitHub, "https://ghe.io");

            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
            foreach (var uri in args)
                Helpers.SetupConnections(hosts, cm, adds, conns, hsts, uri);

            foreach (var host in hsts)
                host.ModelService.GetAccounts().Returns(Observable.Return(new ReactiveList<IAccount>()));

            cm.Connections.Returns(new ObservableCollection<IConnection>(conns));

            var vsServices = Substitute.For<IVSServices>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.GetViewModel(hosts, repositoryPublishService, vsServices, cm);

            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(ProgressState.Fail));

            vm.SelectedConnection = conns.First(x => x != vm.SelectedConnection);

            vsServices.Received().ClearNotifications();
        }

        [Fact]
        public async Task ClearsErrorsWhenSwitchingAccounts()
        {
            var cm = Substitutes.ConnectionManager;
            var hosts = Substitute.For<IRepositoryHosts>();
            var adds = new List<HostAddress>();
            var hsts = new List<IRepositoryHost>();
            var conns = new List<IConnection>();
                Helpers.SetupConnections(hosts, cm, adds, conns, hsts, GitHubUrls.GitHub);

            var accounts = new ReactiveList<IAccount> { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
            hsts[0].ModelService.GetAccounts().Returns(Observable.Return(accounts));

            cm.Connections.Returns(new ObservableCollection<IConnection>(conns));

            var vsServices = Substitute.For<IVSServices>();

            var repositoryPublishService = Substitute.For<IRepositoryPublishService>();
            repositoryPublishService.PublishRepository(Args.NewRepository, Args.Account, Args.ApiClient)
                .Returns(Observable.Throw<Octokit.Repository>(new Octokit.RepositoryExistsException("repo-name", new Octokit.ApiValidationException())));
            var vm = Helpers.GetViewModel(hosts, repositoryPublishService, vsServices, cm);

            vm.RepositoryName = "repo-name";

            await vm.PublishRepository.ExecuteAsync().Catch(Observable.Return(ProgressState.Fail));

            vm.SelectedAccount = accounts[1];

            vsServices.Received().ClearNotifications();
        }
    }
}
