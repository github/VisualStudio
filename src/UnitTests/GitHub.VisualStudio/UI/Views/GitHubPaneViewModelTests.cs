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
using System.Windows.Input;
using GitHub.ViewModels;
using System.ComponentModel;

public class GitHubPaneViewModelTests : TestBaseClass
{
    readonly IServiceProvider serviceProvider;
    readonly IUIController uiController;
    readonly FakeMenuCommandService menuCommandService;
    readonly GitHubPaneViewModel viewModel;
    UIViewType lastUiControllerJump;

    public GitHubPaneViewModelTests()
    {
        var repositoryHosts = Substitutes.RepositoryHosts;
        repositoryHosts.IsLoggedInToAnyHost.Returns(true);

        var teamExplorerServiceHolder = Substitute.For<ITeamExplorerServiceHolder>();
        var activeRepo = Substitute.For<ILocalRepositoryModel>();
        activeRepo.CloneUrl.Returns(new UriString("https://github.com/foo/foo"));
        teamExplorerServiceHolder
            .When(x => x.Subscribe(Arg.Any<object>(), Arg.Any<Action<ILocalRepositoryModel>>()))
            .Do(x =>
            {
                var invokeAction = x.Arg<Action<ILocalRepositoryModel>>();
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

        serviceProvider = Substitutes.GetFullyMockedServiceProvider();
        menuCommandService = new FakeMenuCommandService();
        serviceProvider.GetService(typeof(IMenuCommandService)).Returns(menuCommandService);

        var uiProvider = serviceProvider as IUIProvider;
        uiProvider.TryGetService(typeof(IUIProvider)).Returns(serviceProvider);

        uiController = Substitute.For<IUIController>();
        uiController.CurrentFlow.Returns(UIControllerFlow.PullRequests);
        uiController.SelectedFlow.Returns(UIControllerFlow.PullRequests);
        uiController
            .When(x => x.Jump(Arg.Any<ViewWithData>()))
            .Do(x => lastUiControllerJump = x.Arg<ViewWithData>().ViewType);

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

    [Fact]
    public void ProxyIsBusyFromViewModel_WhenICanBeBusy()
    {
        var hostedViewModel = new CanBeBusyBasicViewModel();
        var view = Substitute.For<IView>();
        view.ViewModel.Returns(hostedViewModel);
        viewModel.Control = view;

        Assert.False(viewModel.IsBusy);

        //ensure we make an change of value atleast once
        hostedViewModel.IsBusy = false;
        hostedViewModel.IsBusy = true;

        Assert.True(viewModel.IsBusy);
    }

    [Fact]
    public void DoNotProxyIsBusyFromViewModel_WhenNotICanBeBusy()
    {
        var hostedViewModel = new BasicViewModel();
        var view = Substitute.For<IView>();
        view.ViewModel.Returns(hostedViewModel);
        viewModel.Control = view;

        Assert.False(viewModel.IsBusy);

        //ensure we make an change of value atleast once
        hostedViewModel.IsBusy = false;
        hostedViewModel.IsBusy = true;

        Assert.False(viewModel.IsBusy);
    }

    private void RunSteps(IEnumerable<NavStep> steps)
    {
        var observableSteps = steps
            .Select(x => new LoadData
            {
                Direction = x.Direction,
                Data = new ViewWithData() { ViewType = x.ViewType },
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

    private class CanBeBusyBasicViewModel : BasicViewModel, GitHub.ViewModels.ICanBeBusy
    {
    }

    private class BasicViewModel : NotificationAwareObject, GitHub.ViewModels.IViewModel
    {
        ICommand cancel;
        public ICommand Cancel
        {
            get { return cancel; }
            set { cancel = value; this.RaisePropertyChanged(); }
        }

        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; this.RaisePropertyChanged(); }
        }

        bool isShowing;
        public bool IsShowing
        {
            get { return isShowing; }
            set { isShowing = value; this.RaisePropertyChanged(); }
        }

        string title;
        public string Title
        {
            get { return title; }
            set { title = value; this.RaisePropertyChanged(); }
        }

        public void Initialize(ViewWithData data)
        {
        }
    }
}
