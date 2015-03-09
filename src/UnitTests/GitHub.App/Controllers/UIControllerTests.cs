using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Controls;
using DesignTimeStyleHelper;
using GitHub.Controllers;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.UI.Views.Controls;
using NSubstitute;
using Xunit;

public class UIControllerTests
{
    public class TheDisposeMethod
    {
        [Fact]
        public void WithMultipleCallsDoesNotThrowException()
        {
            var uiProvider = Substitute.For<IUIProvider>();
            var hosts = Substitute.For<IRepositoryHosts>();
            var factory = new ExportFactoryProvider(Substitute.For<ICompositionService>());
            var uiController = new UIController(uiProvider, hosts, factory);

            uiController.Dispose();
            uiController.Dispose();
        }
    }

    public class TheStartMethod
    {
        [Fact]
        public void ShowingCloneDialogWithoutBeingLoggedInShowsLoginDialog()
        {
            var uiProvider = Substitute.For<IUIProvider>();
            var hosts = Substitute.For<IRepositoryHosts>();
            var factory = (ExportFactoryProvider)new CustomServiceProvider().GetService(typeof(ExportFactoryProvider));
            UserControl shownControl = null;
            using (var uiController = new UIController(uiProvider, hosts, factory))
            {
                uiController.SelectFlow(UIControllerFlow.Clone).Subscribe(uc => shownControl = uc);

                uiController.Start();

                Assert.IsType<LoginControl>(shownControl);
            }
        }

        [Fact]
        public void ShowingCloneDialogWhenLoggedInShowsCloneDialog()
        {
            var uiProvider = Substitute.For<IUIProvider>();
            var hosts = Substitute.For<IRepositoryHosts>();
            hosts.IsLoggedInToAnyHost.Returns(true);
            var factory = (ExportFactoryProvider)new CustomServiceProvider().GetService(typeof(ExportFactoryProvider));
            UserControl shownControl = null;
            using (var uiController = new UIController(uiProvider, hosts, factory))
            {
                uiController.SelectFlow(UIControllerFlow.Clone).Subscribe(uc => shownControl = uc);

                uiController.Start();

                Assert.IsType<CloneRepositoryControl>(shownControl);
            }
        }
    }
}
