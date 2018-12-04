using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using ReactiveUI;
using NUnit.Framework;

public class GitHubPaneViewModelTests : TestBaseClass
{
    const string ValidGitHubRepo = "https://github.com/owner/repo";
    const string ValidEnterpriseRepo = "https://enterprise.com/owner/repo";

    public class TheInitializeMethod
    {
        [Test]
        public async Task NotAGitRepositoryShownWhenNoRepositoryAsync()
        {
            var te = Substitute.For<ITeamExplorerContext>();
            te.ActiveRepository.Returns(null as LocalRepositoryModel);
            var target = CreateTarget(teamExplorerContext: te);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<INotAGitRepositoryViewModel>());
        }

        [Test]
        public async Task NotAGitHubRepositoryShownWhenRepositoryCloneUrlIsNullAsync()
        {
            var te = CreateTeamExplorerContext(null);
            var target = CreateTarget(teamExplorerContext: te);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<INotAGitHubRepositoryViewModel>());
        }

        [Test]
        public async Task NotAGitHubRepositoryShownWhenRepositoryIsNotAGitHubInstanceAsync()
        {
            var te = CreateTeamExplorerContext("https://some.site/foo/bar");
            var target = CreateTarget(teamExplorerContext: te);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<INotAGitHubRepositoryViewModel>());
        }

        [Test]
        public async Task NotAGitHubRepositoryShownWhenRepositoryIsADeletedGitHubRepoAsync()
        {
            var te = CreateTeamExplorerContext("https://github.com/invalid/repo");
            var cm = CreateConnectionManager("https://github.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<INotAGitHubRepositoryViewModel>());
        }

        [Test]
        public async Task LoggedOutShownWhenNotLoggedInToGitHubAsync()
        {
            var te = CreateTeamExplorerContext(ValidGitHubRepo);
            var cm = CreateConnectionManager("https://enterprise.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<ILoggedOutViewModel>());
        }

        [Test]
        public async Task LoginFailedShownWhenConnectionHasError()
        {
            var te = CreateTeamExplorerContext(ValidGitHubRepo);
            var exception = new Exception();
            var cm = CreateConnectionManager(exception, "https://github.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<ILoginFailedViewModel>());
        }

        [Test]
        public async Task LoggedOutShownWhenNotLoggedInToEnterpriseAsync()
        {
            var te = CreateTeamExplorerContext(ValidEnterpriseRepo);
            var cm = CreateConnectionManager("https://github.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<ILoggedOutViewModel>());
        }

        [Test]
        public async Task NavigatorShownWhenRepositoryIsAGitHubRepoAsync()
        {
            var cm = CreateConnectionManager("https://github.com");
            var target = CreateTarget(connectionManager: cm);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<INavigationViewModel>());
        }

        [Test]
        public async Task NavigatorShownWhenRepositoryIsAnEnterpriseRepoAsync()
        {
            var te = CreateTeamExplorerContext(ValidEnterpriseRepo);
            var cm = CreateConnectionManager("https://enterprise.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<INavigationViewModel>());
        }

        [Test]
        public async Task NavigatorShownWhenUserLogsInAsync()
        {
            var cm = CreateConnectionManager();
            var target = CreateTarget(connectionManager: cm);

            await InitializeAsync(target);

            Assert.That(target.Content, Is.InstanceOf<ILoggedOutViewModel>());

            AddConnection(cm, "https://github.com");

            Assert.That(target.Content, Is.InstanceOf<INavigationViewModel>());
        }
    }

    public class TheShowPullRequestsMethod
    {
        [Test]
        public async Task HasNoEffectWhenUserLoggedOutAsync()
        {
            var viewModelFactory = Substitute.For<IViewViewModelFactory>();
            var target = CreateTarget(
                viewModelFactory: viewModelFactory,
                connectionManager: CreateConnectionManager());

            await InitializeAsync(target);
            Assert.That(target.Content, Is.InstanceOf<ILoggedOutViewModel>());

            await target.ShowPullRequests();

            viewModelFactory.DidNotReceive().CreateViewModel<IPullRequestListViewModel>();
        }

        [Test]
        public async Task HasNoEffectWhenAlreadyCurrentPageAsync()
        {
            var cm = CreateConnectionManager(ValidGitHubRepo);
            var nav = new NavigationViewModel();
            var target = CreateTarget(
                connectionManager: cm,
                navigator: nav);

            await InitializeAsync(target);
            Assert.That(nav, Is.SameAs(target.Content));
            Assert.That(nav.Content, Is.InstanceOf<IPullRequestListViewModel>());

            await target.ShowPullRequests();

            Assert.That(1, Is.EqualTo(nav.History.Count));
        }
    }

    static GitHubPaneViewModel CreateTarget(
        IViewViewModelFactory viewModelFactory = null,
        ISimpleApiClientFactory apiClientFactory = null,
        IConnectionManager connectionManager = null,
        ITeamExplorerContext teamExplorerContext = null,
        IVisualStudioBrowser browser = null,
        IUsageTracker usageTracker = null,
        INavigationViewModel navigator = null,
        ILoggedOutViewModel loggedOut = null,
        INotAGitHubRepositoryViewModel notAGitHubRepository = null,
        INotAGitRepositoryViewModel notAGitRepository = null,
        INoRemoteOriginViewModel noRemoteOriginViewModel = null,
        ILoginFailedViewModel loginFailed = null)
    {
        viewModelFactory = viewModelFactory ?? Substitute.For<IViewViewModelFactory>();
        connectionManager = connectionManager ?? Substitute.For<IConnectionManager>();
        teamExplorerContext = teamExplorerContext ?? CreateTeamExplorerContext(ValidGitHubRepo);
        browser = browser ?? Substitute.For<IVisualStudioBrowser>();
        usageTracker = usageTracker ?? Substitute.For<IUsageTracker>();
        loggedOut = loggedOut ?? Substitute.For<ILoggedOutViewModel>();
        notAGitHubRepository = notAGitHubRepository ?? Substitute.For<INotAGitHubRepositoryViewModel>();
        notAGitRepository = notAGitRepository ?? Substitute.For<INotAGitRepositoryViewModel>();
        noRemoteOriginViewModel = noRemoteOriginViewModel ?? Substitute.For<INoRemoteOriginViewModel>();
        loginFailed = loginFailed ?? Substitute.For<ILoginFailedViewModel>();

        if (navigator == null)
        {
            navigator = CreateNavigator();
            navigator.Content.Returns((IPanePageViewModel)null);
        }

        if (apiClientFactory == null)
        {
            var validGitHubRepoClient = Substitute.For<ISimpleApiClient>();
            var validEnterpriseRepoClient = Substitute.For<ISimpleApiClient>();
            var invalidRepoClient = Substitute.For<ISimpleApiClient>();

            validGitHubRepoClient.GetRepository().Returns(new Octokit.Repository(1));
            validEnterpriseRepoClient.GetRepository().Returns(new Octokit.Repository(1));
            validEnterpriseRepoClient.IsEnterprise().Returns(true);

            apiClientFactory = Substitute.For<ISimpleApiClientFactory>();
            apiClientFactory.Create(null).ReturnsForAnyArgs(invalidRepoClient);
            apiClientFactory.Create(ValidGitHubRepo).Returns(validGitHubRepoClient);
            apiClientFactory.Create(ValidEnterpriseRepo).Returns(validEnterpriseRepoClient);
        }

        return new GitHubPaneViewModel(
            viewModelFactory,
            apiClientFactory,
            connectionManager,
            teamExplorerContext,
            browser,
            usageTracker,
            navigator,
            loggedOut,
            notAGitHubRepository,
            notAGitRepository,
            noRemoteOriginViewModel,
            loginFailed);
    }

    static IConnectionManager CreateConnectionManager(params string[] addresses)
    {
        return CreateConnectionManager(null, addresses);
    }

    static IConnectionManager CreateConnectionManager(Exception loginError, params string[] addresses)
    {
        var result = Substitute.For<IConnectionManager>();
        var connections = new ObservableCollectionEx<IConnection>();

        result.Connections.Returns(connections);
        result.GetLoadedConnections().Returns(connections);
        result.GetConnection(null).ReturnsForAnyArgs(default(IConnection));

        foreach (var address in addresses)
        {
            AddConnection(result, address, loginError);
        }

        return result;
    }

    static void AddConnection(IConnectionManager connectionManager, string address, Exception loginError = null)
    {
        var connection = Substitute.For<IConnection>();
        var hostAddress = HostAddress.Create(address);
        var connections = (ObservableCollectionEx<IConnection>)connectionManager.Connections;
        connection.HostAddress.Returns(hostAddress);
        connection.IsLoggedIn.Returns(loginError == null);
        connection.ConnectionError.Returns(loginError);
        connectionManager.GetConnection(hostAddress).Returns(connection);
        connections.Add(connection);
    }

    static INavigationViewModel CreateNavigator()
    {
        var result = Substitute.For<INavigationViewModel>();
        result.NavigateBack.Returns(ReactiveCommand.Create(() => { }));
        result.NavigateForward.Returns(ReactiveCommand.Create(() => { }));
        return result;
    }

    static ITeamExplorerContext CreateTeamExplorerContext(string repositoryCloneUrl)
    {
        var repository = new LocalRepositoryModel
        {
            CloneUrl = new UriString(repositoryCloneUrl)
        };
        var result = Substitute.For<ITeamExplorerContext>();
        result.ActiveRepository.Returns(repository);
        return result;
    }

    static async Task InitializeAsync(GitHubPaneViewModel target)
    {
        var paneServiceProvider = Substitute.For<IServiceProvider>();
        var menuCommandService = Substitute.For<IMenuCommandService>();
        paneServiceProvider.GetService(typeof(IMenuCommandService)).Returns(menuCommandService);
        await target.InitializeAsync(paneServiceProvider);
    }
}
