using System.ComponentModel.Composition;
using GitHub.Controllers;
using GitHub.Models;
using GitHub.Services;
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
}
