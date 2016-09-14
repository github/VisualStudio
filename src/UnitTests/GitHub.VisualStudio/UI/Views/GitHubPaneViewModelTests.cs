using GitHub.Api;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio;
using GitHub.VisualStudio.UI.Views;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;
using System.Reactive.Linq;
using UnitTests;
using Xunit;

public class GitHubPaneViewModelTests : TestBaseClass
{
    private readonly IServiceProvider serviceProvider;
    private readonly IUIController uiController;
    private readonly FakeMenuCommandService menuCommandService;
    private readonly GitHubPaneViewModel viewModel;
    private UIViewType lastUiControllerJump;

    public GitHubPaneViewModelTests()
    {
        #region Start tests already logged in with an active repo.

        var repositoryHosts = Substitutes.RepositoryHosts;
        repositoryHosts.IsLoggedInToAnyHost.Returns(true);

        var teamExplorerServiceHolder = Substitute.For<ITeamExplorerServiceHolder>();
        var activeRepo = Substitute.For<ILocalRepositoryModel>();
        activeRepo.CloneUrl.Returns(new UriString("https://github.com/foo/foo"));
        teamExplorerServiceHolder
            .When(_ => _.Subscribe(Arg.Any<object>(), Arg.Any<Action<ILocalRepositoryModel>>()))
            .Do(_ =>
            {
                var invokeAction = _.Arg<Action<ILocalRepositoryModel>>();
                invokeAction(activeRepo);
            });

        var connectionManager = Substitutes.ConnectionManager;
        var connection = Substitutes.Connection;
        var connectionHostAddress = HostAddress.Create(activeRepo.CloneUrl.ToString());
        connection.HostAddress.Returns(connectionHostAddress);
        connectionManager.Connections.Returns(new ObservableCollection<IConnection>(new[] {
                connection
            }));
        connection.Login().Returns(Observable.Return(connection));

        var host = Substitute.For<IRepositoryHost>();
        host.IsLoggedIn.Returns(true);
        repositoryHosts.LookupHost(connectionHostAddress).Returns(host);

        #endregion

        #region Wire up service/UI provider.

        serviceProvider = Substitutes.GetFullyMockedServiceProvider();
        menuCommandService = new FakeMenuCommandService();
        serviceProvider.GetService(typeof(IMenuCommandService)).Returns(menuCommandService);

        // TODO: Maybe could avoid cast by making a new interface that inherits IServiceProvider and IUIProvider
        var uiProvider = serviceProvider as IUIProvider;
        uiProvider.TryGetService(typeof(IUIProvider)).Returns(serviceProvider);

        #endregion

        uiController = Substitute.For<IUIController>();
        uiController.CurrentFlow.Returns(UIControllerFlow.PullRequests);
        uiController.SelectedFlow.Returns(UIControllerFlow.PullRequests);
        uiController
            .When(_ => _.Jump(Arg.Any<ViewWithData>()))
            .Do(_ => lastUiControllerJump = _.Arg<ViewWithData>().ViewType);

        var exportFactoryProvider = Substitutes.ExportFactoryProvider;
        uiProvider.TryGetService(typeof(IExportFactoryProvider)).Returns(exportFactoryProvider);
        exportFactoryProvider.UIControllerFactory.Returns(new ExportFactory<IUIController>(
            () => Tuple.Create<IUIController, Action>(uiController, () => { })));

        viewModel = new GitHubPaneViewModel(
            Substitute.For<ISimpleApiClientFactory>(),
            teamExplorerServiceHolder,
            connectionManager,
            repositoryHosts,
            Substitute.For<INotificationDispatcher>());

        viewModel.ActiveRepo = activeRepo;
    }

    [Fact]
    public void ListRefreshKeepsListVisible_DoesNotSwitchToPRCreation()
    {
        RunSteps(new[]
        {
            new NavStep(LoadDirection.Forward, UIViewType.PRList),
            new NavStep(LoadDirection.Forward, UIViewType.PRCreation),
            new NavStep(LoadDirection.Back, UIViewType.PRList),
            new NavStep(LoadDirection.Forward, UIViewType.PRCreation),
            new NavStep(LoadDirection.Forward, UIViewType.PRList),
        });

        menuCommandService.ExecuteCommand(PkgCmdIDList.refreshCommand);

        Assert.Equal(UIViewType.PRList, lastUiControllerJump);
    }

    private void RunSteps(IEnumerable<NavStep> steps)
    {
        var observableSteps = steps
            .Select(_ => new LoadData
            {
                Direction = _.Direction,
                Data = new ViewWithData() { ViewType = _.ViewType },
                View = Substitute.For<IView>()
            })
            .ToObservable();

        uiController.SelectFlow(UIControllerFlow.PullRequests).Returns(observableSteps);

        viewModel.Initialize(serviceProvider);
    }

    private class NavStep
    {
        public NavStep(LoadDirection direction, UIViewType viewtype)
        {
            Direction = direction;
            ViewType = viewtype;
        }

        public LoadDirection Direction { get; private set; }
        public UIViewType ViewType { get; private set; }
    }
}
