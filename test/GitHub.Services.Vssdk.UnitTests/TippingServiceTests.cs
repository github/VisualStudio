using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using GitHub.Services.Vssdk.Services;
using NSubstitute;
using POINT = Microsoft.VisualStudio.OLE.Interop.POINT;

public class TippingServiceTests
{
    public class TheRequestCalloutDisplayMethod
    {
        [Test]
        public void No_Exception_When_Cant_Find_SVsTippingService()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            var target = new TippingService(serviceProvider);

            Assert.DoesNotThrow(() =>
                target.RequestCalloutDisplay(Guid.Empty, "title", "message", true, null, Guid.Empty, 0));
        }

        [Test]
        public void No_Exception_When_Api_Has_Changed()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(null).ReturnsForAnyArgs(new object());
            var target = new TippingService(serviceProvider);

            Assert.DoesNotThrow(() =>
                target.RequestCalloutDisplay(Guid.Empty, "title", "message", true, null, Guid.Empty, 0));
        }

        [Test]
        public void Check_Arguments_Passed_To_RequestCalloutDisplay()
        {
            var calloutId = Guid.NewGuid();
            var title = "title";
            var message = "message";
            var isPermanentlyDismissable = true;
            var commandGroupId = Guid.NewGuid();
            uint commandId = 777;
            var serviceProvider = Substitute.For<IServiceProvider>();
            var service = Substitute.For<IVsTippingService>();
            serviceProvider.GetService(null).ReturnsForAnyArgs(service);
            var target = new TippingService(serviceProvider);

            target.RequestCalloutDisplay(calloutId, title, message, isPermanentlyDismissable, default, commandGroupId, commandId);

            service.Received(1).RequestCalloutDisplay(TippingService.ClientId, calloutId, title, message, isPermanentlyDismissable,
                default, commandGroupId, commandId);
        }

        [Guid("756F1DC9-47FA-42C5-9C06-252B54148EB8")]
        public interface IVsTippingService
        {
            void RequestCalloutDisplay(Guid clientId, Guid calloutId, string title, string message, bool isPermanentlyDismissible,
                POINT anchor, Guid vsCommandGroupId, uint vsCommandId);
        }
    }
}
