using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Controls;
using GitHub.Controllers;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NSubstitute;
using Xunit;
using UnitTests;
using GitHub.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using GitHub.Authentication;
using System.Collections.ObjectModel;

public class UIControllerTests
{
    public class TheDisposeMethod : TestBaseClass
    {
        [Fact]
        public void WithMultipleCallsDoesNotThrowException()
        {
            var uiProvider = Substitute.For<IUIProvider>();
            var hosts = Substitute.For<IRepositoryHosts>();
            var factory = Substitute.For<IExportFactoryProvider>();
            var cm = Substitutes.ConnectionManager;
            var uiController = new UIController(uiProvider, hosts, factory, cm, LazySubstitute.For<ITwoFactorChallengeHandler>());

            uiController.Dispose();
            uiController.Dispose();
        }
    }

    public class TheStartMethod : TestBaseClass
    {
        IExportFactoryProvider SetupFactory(IServiceProvider provider)
        {
            var factory = provider.GetExportFactoryProvider();
            factory.GetViewModel(GitHub.Exports.UIViewType.Login).Returns(new ExportLifetimeContext<IViewModel>(Substitute.For<IViewModel>(), () => { }));
            factory.GetView(GitHub.Exports.UIViewType.Login).Returns(new ExportLifetimeContext<IView>(Substitute.For<IView, IViewFor<ILoginControlViewModel>, SimpleViewUserControl>(), () => { }));
            factory.GetViewModel(GitHub.Exports.UIViewType.TwoFactor).Returns(new ExportLifetimeContext<IViewModel>(Substitute.For<IViewModel>(), () => { }));
            factory.GetView(GitHub.Exports.UIViewType.TwoFactor).Returns(new ExportLifetimeContext<IView>(Substitute.For<IView, IViewFor<ITwoFactorDialogViewModel>, SimpleViewUserControl>(), () => { }));
            factory.GetViewModel(GitHub.Exports.UIViewType.Clone).Returns(new ExportLifetimeContext<IViewModel>(Substitute.For<IViewModel>(), () => { }));
            factory.GetView(GitHub.Exports.UIViewType.Clone).Returns(new ExportLifetimeContext<IView>(Substitute.For<IView, IViewFor<IRepositoryCloneViewModel>, SimpleViewUserControl>(), () => { }));
            return factory;
        }

        [STAFact]
        public void ShowingCloneDialogWithoutBeingLoggedInShowsLoginDialog()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var loginView = factory.GetView(GitHub.Exports.UIViewType.Login);
            loginView.Value.Cancel.Returns(Observable.Empty<object>());
            var cm = provider.GetConnectionManager();
            cm.Connections.Returns(new ObservableCollection<IConnection>());

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm, LazySubstitute.For<ITwoFactorChallengeHandler>()))
            {
                var list = new List<IView>();
                uiController.SelectFlow(UIControllerFlow.Clone)
                    .Subscribe(uc => list.Add(uc as IView),
                                () =>
                                {
                                    Assert.True(list.Count > 1);
                                    Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(list[0]);
                                });

                uiController.Start(null);
            }
        }

        [STAFact]
        public void ShowingCloneDialogWhenLoggedInShowsCloneDialog()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var connection = provider.GetConnection();
            connection.Login().Returns(Observable.Return(connection));
            var cm = provider.GetConnectionManager();
            cm.Connections.Returns(new ObservableCollection<IConnection> { connection });
            var host = hosts.GitHubHost;
            hosts.LookupHost(connection.HostAddress).Returns(host);
            host.IsLoggedIn.Returns(true);

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm, LazySubstitute.For<ITwoFactorChallengeHandler>()))
            {
                var list = new List<IView>();
                uiController.SelectFlow(UIControllerFlow.Clone)
                    .Subscribe(uc => list.Add(uc as IView),
                                () =>
                                {
                                    Assert.Equal(1, list.Count);
                                    Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(list[0]);
                                });
                uiController.Start(connection);
            }
        }

        [STAFact]
        public void CloneDialogLoggedInWithoutConnection()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var connection = provider.GetConnection();
            connection.Login().Returns(Observable.Return(connection));
            var cm = provider.GetConnectionManager();
            cm.Connections.Returns(new ObservableCollection<IConnection> { connection });
            var host = hosts.GitHubHost;
            hosts.LookupHost(connection.HostAddress).Returns(host);
            host.IsLoggedIn.Returns(true);

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm, LazySubstitute.For<ITwoFactorChallengeHandler>()))
            {
                var list = new List<IView>();
                uiController.SelectFlow(UIControllerFlow.Clone)
                    .Subscribe(uc => list.Add(uc as IView),
                                () =>
                                {
                                    Assert.Equal(1, list.Count);
                                    Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(list[0]);
                                    ((IUIProvider)provider).Received().AddService(uiController, connection);
                                });
                uiController.Start(null);
            }
        }
    }
}
