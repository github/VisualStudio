using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using GitHub.Services.Vssdk.Services;
using NSubstitute;

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
    }
}
