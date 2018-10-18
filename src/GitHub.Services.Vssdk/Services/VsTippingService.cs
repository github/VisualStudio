using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft;
using IServiceProvider = System.IServiceProvider;

namespace GitHub.Services.Vssdk.Services
{
    public class VsTippingService : IVsTippingService
    {
        // This is the only supported ClientId
        readonly Guid ClientId = new Guid("D5D3B674-05BB-4942-B8EC-C3D13B5BD6EE");

        readonly IServiceProvider serviceProvider;

        public VsTippingService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void RequestCalloutDisplay(Guid calloutId, string title, string message,
            bool isPermanentlyDismissible, FrameworkElement targetElement,
            Guid vsCommandGroupId, uint vsCommandId)
        {
            var screenPoint = targetElement.PointToScreen(new Point(targetElement.ActualWidth / 2, 0));
            var point = new Microsoft.VisualStudio.OLE.Interop.POINT { x = (int)screenPoint.X, y = (int)screenPoint.Y };
            RequestCalloutDisplay(ClientId, calloutId, title, message, isPermanentlyDismissible,
                point, vsCommandGroupId, vsCommandId);
        }

        // Available on Visual Studio 2017
        void RequestCalloutDisplay(Guid clientId, Guid calloutId, string title, string message,
            bool isPermanentlyDismissible, FrameworkElement targetElement,
            Guid vsCommandGroupId, uint vsCommandId, object commandOption = null)
        {
            var tippingService = serviceProvider.GetService(typeof(SVsTippingService));
            Assumes.Present(tippingService);
            var currentMethod = MethodBase.GetCurrentMethod();
            var parameterTypes = currentMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            var method = tippingService.GetType().GetMethod(currentMethod.Name, parameterTypes);
            var arguments = new object[] { clientId, calloutId, title, message, isPermanentlyDismissible, targetElement,
                    vsCommandGroupId, vsCommandId, commandOption };
            method.Invoke(tippingService, arguments);
        }

        // Available on Visual Studio 2015
        void RequestCalloutDisplay(Guid clientId, Guid calloutId, string title, string message, bool isPermanentlyDismissible,
            Microsoft.VisualStudio.OLE.Interop.POINT anchor, Guid vsCommandGroupId, uint vsCommandId)
        {
            var tippingService = serviceProvider.GetService(typeof(SVsTippingService));
            Assumes.Present(tippingService);
            var currentMethod = MethodBase.GetCurrentMethod();
            var parameterTypes = currentMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            var method = tippingService.GetType().GetMethod(currentMethod.Name, parameterTypes);
            var arguments = new object[] { clientId, calloutId, title, message, isPermanentlyDismissible, anchor,
                    vsCommandGroupId, vsCommandId };
            method.Invoke(tippingService, arguments);
        }
    }

    [Guid("DCCC6A2B-F300-4DA1-92E1-8BF4A5BCA795")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [TypeIdentifier]
    [ComImport]
    public interface SVsTippingService
    {
    }
}
