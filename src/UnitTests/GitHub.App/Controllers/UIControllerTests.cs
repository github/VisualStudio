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

public class UIControllerTests
{
    public class TheDisposeMethod
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

    public class TheStartMethod
    {
        IExportFactoryProvider SetupFactory(IServiceProvider provider)
        {
            var factory = provider.GetExportFactoryProvider();
            factory.GetViewModel(GitHub.Exports.UIViewType.Login).Returns(new ExportLifetimeContext<IViewModel>(Substitute.For<IViewModel>(), () => { }));
            factory.GetView(GitHub.Exports.UIViewType.Login).Returns(new ExportLifetimeContext<IView>(Substitute.For<IView, IViewFor<ILoginControlViewModel>, UserControl>(), () => { }));
            factory.GetViewModel(GitHub.Exports.UIViewType.TwoFactor).Returns(new ExportLifetimeContext<IViewModel>(Substitute.For<IViewModel>(), () => { }));
            factory.GetView(GitHub.Exports.UIViewType.TwoFactor).Returns(new ExportLifetimeContext<IView>(Substitute.For<IView, IViewFor<ITwoFactorDialogViewModel>, UserControl>(), () => { }));
            factory.GetViewModel(GitHub.Exports.UIViewType.Clone).Returns(new ExportLifetimeContext<IViewModel>(Substitute.For<IViewModel>(), () => { }));
            factory.GetView(GitHub.Exports.UIViewType.Clone).Returns(new ExportLifetimeContext<IView>(Substitute.For<IView, IViewFor<IRepositoryCloneViewModel>, UserControl>(), () => { }));
            return factory;
        }

        [STAFact]
        public void ShowingCloneDialogWithoutBeingLoggedInShowsLoginDialog()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var cm = provider.GetConnectionManager();
            var cons = new System.Collections.ObjectModel.ObservableCollection<IConnection>();
            cm.Connections.Returns(cons);

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm, LazySubstitute.For<ITwoFactorChallengeHandler>()))
            {
                var list = new List<IView>();
                uiController.SelectFlow(UIControllerFlow.Clone, null)
                    .Subscribe(uc => list.Add(uc as IView),
                                () =>
                                {
                                    Assert.True(list.Count > 1);
                                    Assert.IsAssignableFrom<IViewFor<ILoginControlViewModel>>(list[0]);
                                });

                uiController.Start();
            }
        }

        [STAFact]
        public void ShowingCloneDialogWhenLoggedInShowsCloneDialog()
        {
            var provider = Substitutes.GetFullyMockedServiceProvider();
            var hosts = provider.GetRepositoryHosts();
            var factory = SetupFactory(provider);
            var connection = provider.GetConnection();
            var cm = provider.GetConnectionManager();
            var host = hosts.GitHubHost;
            hosts.LookupHost(connection.HostAddress).Returns(host);
            host.IsLoggedIn.Returns(true);

            using (var uiController = new UIController((IUIProvider)provider, hosts, factory, cm, LazySubstitute.For<ITwoFactorChallengeHandler>()))
            {
                var list = new List<IView>();
                uiController.SelectFlow(UIControllerFlow.Clone, connection)
                    .Subscribe(uc => list.Add(uc as IView),
                                () =>
                                {
                                    Assert.Equal(1, list.Count);
                                    Assert.IsAssignableFrom<IViewFor<IRepositoryCloneViewModel>>(list[0]);
                                });
                uiController.Start();
            }
        }
    }
}
