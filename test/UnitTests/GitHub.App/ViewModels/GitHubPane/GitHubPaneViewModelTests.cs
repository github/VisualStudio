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
using Xunit;

public class GitHubPaneViewModelTests : TestBaseClass
{
    const string ValidGitHubRepo = "https://github.com/owner/repo";
    const string ValidEnterpriseRepo = "https://enterprise.com/owner/repo";

    public class TheInitializeMethod
    {
        [Fact]
        public async Task NotAGitRepositoryShownWhenNoRepository()
        {
            var te = Substitute.For<ITeamExplorerContext>();
            te.ActiveRepository.Returns(default(ILocalRepositoryModel));
            var target = CreateTarget(teamExplorerContext: te);

            await Initialize(target);

            Assert.IsAssignableFrom<INotAGitRepositoryViewModel>(target.Content);
        }

        [Fact]
        public async Task NotAGitHubRepositoryShownWhenRepositoryCloneUrlIsNull()
        {
            var repo = Substitute.For<ILocalRepositoryModel>();
            var te = CreateTeamExplorerContext(null);
            var target = CreateTarget(teamExplorerContext: te);

            await Initialize(target);

            Assert.IsAssignableFrom<INotAGitHubRepositoryViewModel>(target.Content);
        }

        [Fact]
        public async Task NotAGitHubRepositoryShownWhenRepositoryIsNotAGitHubInstance()
        {
            var te = CreateTeamExplorerContext("https://some.site/foo/bar");
            var target = CreateTarget(teamExplorerContext: te);

            await Initialize(target);

            Assert.IsAssignableFrom<INotAGitHubRepositoryViewModel>(target.Content);
        }

        [Fact]
        public async Task NotAGitHubRepositoryShownWhenRepositoryIsADeletedGitHubRepo()
        {
            var te = CreateTeamExplorerContext("https://github.com/invalid/repo");
            var target = CreateTarget(teamExplorerContext: te);

            await Initialize(target);

            Assert.IsAssignableFrom<INotAGitHubRepositoryViewModel>(target.Content);
        }

        [Fact]
        public async Task LoggedOutShownWhenNotLoggedInToGitHub()
        {
            var te = CreateTeamExplorerContext(ValidGitHubRepo);
            var cm = CreateConnectionManager("https://enterprise.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await Initialize(target);

            Assert.IsAssignableFrom<ILoggedOutViewModel>(target.Content);
        }

        [Fact]
        public async Task LoggedOutShownWhenNotLoggedInToEnterprise()
        {
            var te = CreateTeamExplorerContext(ValidEnterpriseRepo);
            var cm = CreateConnectionManager("https://github.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await Initialize(target);

            Assert.IsAssignableFrom<ILoggedOutViewModel>(target.Content);
        }

        [Fact]
        public async Task NavigatorShownWhenRepositoryIsAGitHubRepo()
        {
            var te = CreateTeamExplorerContext(ValidGitHubRepo);
            var cm = CreateConnectionManager("https://github.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await Initialize(target);

            Assert.IsAssignableFrom<INavigationViewModel>(target.Content);
        }

        [Fact]
        public async Task NavigatorShownWhenRepositoryIsAnEnterpriseRepo()
        {
            var te = CreateTeamExplorerContext(ValidEnterpriseRepo);
            var cm = CreateConnectionManager("https://enterprise.com");
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await Initialize(target);

            Assert.IsAssignableFrom<INavigationViewModel>(target.Content);
        }

        [Fact]
        public async Task NavigatorShownWhenUserLogsIn()
        {
            var te = CreateTeamExplorerContext(ValidGitHubRepo);
            var cm = CreateConnectionManager();
            var target = CreateTarget(teamExplorerContext: te, connectionManager: cm);

            await Initialize(target);

            Assert.IsAssignableFrom<ILoggedOutViewModel>(target.Content);

            AddConnection(cm, "https://github.com");

            Assert.IsAssignableFrom<INavigationViewModel>(target.Content);
        }
    }

    public class TheShowPullRequestsMethod
    {
        [Fact]
        public async Task HasNoEffectWhenUserLoggedOut()
        {
            var te = CreateTeamExplorerContext(ValidGitHubRepo);
            var viewModelFactory = Substitute.For<IViewViewModelFactory>();
            var target = CreateTarget(
                viewModelFactory: viewModelFactory,
                connectionManager: CreateConnectionManager(),
                teamExplorerContext: te);

            await Initialize(target);
            Assert.IsAssignableFrom<ILoggedOutViewModel>(target.Content);

            await target.ShowPullRequests();

            viewModelFactory.DidNotReceive().CreateViewModel<IPullRequestListViewModel>();
        }

        [Fact]
        public async Task HasNoEffectWhenAlreadyCurrentPage()
        {
            var te = CreateTeamExplorerContext(ValidGitHubRepo);
            var cm = CreateConnectionManager(ValidGitHubRepo);
            var nav = new NavigationViewModel();
            var target = CreateTarget(
                teamExplorerContext: te,
                connectionManager: cm,
                navigator: nav);

            await Initialize(target);
            Assert.Same(nav, target.Content);
            Assert.IsAssignableFrom<IPullRequestListViewModel>(nav.Content);

            await target.ShowPullRequests();

            Assert.Equal(1, nav.History.Count);
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
        INotAGitRepositoryViewModel notAGitRepository = null)
    {
        viewModelFactory = viewModelFactory ?? Substitute.For<IViewViewModelFactory>();
        connectionManager = connectionManager ?? Substitute.For<IConnectionManager>();
        teamExplorerContext = teamExplorerContext ?? Substitute.For<ITeamExplorerContext>();
        browser = browser ?? Substitute.For<IVisualStudioBrowser>();
        usageTracker = usageTracker ?? Substitute.For<IUsageTracker>();
        loggedOut = loggedOut ?? Substitute.For<ILoggedOutViewModel>();
        notAGitHubRepository = notAGitHubRepository ?? Substitute.For<INotAGitHubRepositoryViewModel>();
        notAGitRepository = notAGitRepository ?? Substitute.For<INotAGitRepositoryViewModel>();

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
            notAGitRepository);
    }

    static IConnectionManager CreateConnectionManager(params string[] addresses)
    {
        var result = Substitute.For<IConnectionManager>();
        var connections = new ObservableCollectionEx<IConnection>();

        result.Connections.Returns(connections);
        result.GetLoadedConnections().Returns(connections);
        result.GetConnection(null).ReturnsForAnyArgs(default(IConnection));

        foreach (var address in addresses)
        {
            AddConnection(result, address);
        }

        return result;
    }

    static void AddConnection(IConnectionManager connectionManager, string address)
    {
        var connection = Substitute.For<IConnection>();
        var hostAddress = HostAddress.Create(address);
        var connections = (ObservableCollectionEx<IConnection>)connectionManager.Connections;
        connection.HostAddress.Returns(hostAddress);
        connection.IsLoggedIn.Returns(true);
        connectionManager.GetConnection(hostAddress).Returns(connection);
        connections.Add(connection);
    }

    static INavigationViewModel CreateNavigator()
    {
        var result = Substitute.For<INavigationViewModel>();
        result.NavigateBack.Returns(ReactiveCommand.Create());
        result.NavigateForward.Returns(ReactiveCommand.Create());
        return result;
    }

    static ITeamExplorerContext CreateTeamExplorerContext(string repositoryCloneUrl)
    {
        var repository = Substitute.For<ILocalRepositoryModel>();
        repository.CloneUrl.Returns(new UriString(repositoryCloneUrl));

        var result = Substitute.For<ITeamExplorerContext>();
        result.ActiveRepository.Returns(repository);
        return result;
    }

    static async Task Initialize(GitHubPaneViewModel target)
    {
        var paneServiceProvider = Substitute.For<IServiceProvider>();
        var menuCommandService = Substitute.For<IMenuCommandService>();
        paneServiceProvider.GetService(typeof(IMenuCommandService)).Returns(menuCommandService);
        await target.InitializeAsync(paneServiceProvider);
    }
}
