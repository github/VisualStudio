using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using GitHub.Logging;
using Microsoft;
using Serilog;
using IServiceProvider = System.IServiceProvider;

namespace GitHub.Services.Vssdk.Services
{
    /// <summary>
    /// This service is a thin wrapper around <see cref="Microsoft.Internal.VisualStudio.Shell.Interop.IVsTippingService"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IVsTippingService"/> interface is public, but contained within the 'Microsoft.VisualStudio.Shell.UI.Internal' assembly.
    /// To avoid a direct dependency on 'Microsoft.VisualStudio.Shell.UI.Internal', we use reflection to call this service.
    /// </remarks>
    public class TippingService : ITippingService
    {
        static readonly ILogger log = LogManager.ForContext<TippingService>();

        // This is the only supported ClientId
        public static readonly Guid ClientId = new Guid("D5D3B674-05BB-4942-B8EC-C3D13B5BD6EE");
        public static readonly Guid IVsTippingServiceGuid = new Guid("756F1DC9-47FA-42C5-9C06-252B54148EB8");

        readonly IServiceProvider serviceProvider;

        public TippingService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public void RequestCalloutDisplay(Guid calloutId, string title, string message,
            bool isPermanentlyDismissible, FrameworkElement targetElement,
            Guid vsCommandGroupId, uint vsCommandId)
        {
            var screenPoint = !Splat.ModeDetector.InUnitTestRunner() ?
                    targetElement.PointToScreen(new Point(targetElement.ActualWidth / 2, 0)) : default;
            var point = new Microsoft.VisualStudio.OLE.Interop.POINT { x = (int)screenPoint.X, y = (int)screenPoint.Y };
            RequestCalloutDisplay(ClientId, calloutId, title, message, isPermanentlyDismissible,
                point, vsCommandGroupId, vsCommandId);
        }

        // Available on Visual Studio 2015
        void RequestCalloutDisplay(Guid clientId, Guid calloutId, string title, string message, bool isPermanentlyDismissible,
            Microsoft.VisualStudio.OLE.Interop.POINT anchor, Guid vsCommandGroupId, uint vsCommandId)
        {
            var tippingService = serviceProvider.GetService(typeof(SVsTippingService));
            if (tippingService == null)
            {
                log.Error("Can't find {ServiceType}", typeof(SVsTippingService));
                return;
            }

            Assumes.Present(tippingService);
            var parameterTypes = new Type[] { typeof(Guid), typeof(Guid), typeof(string), typeof(string), typeof(bool),
                typeof(Microsoft.VisualStudio.OLE.Interop.POINT), typeof(Guid), typeof(uint) };
            var tippingServiceType = tippingService.GetType();
            var method = tippingServiceType.GetInterfaces()
                .FirstOrDefault(i => i.GUID == IVsTippingServiceGuid)?.GetMethod("RequestCalloutDisplay",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                    null, parameterTypes, null);
            if (method == null)
            {
                log.Error("Couldn't find method on {Type} with parameters {Parameters}", tippingServiceType, parameterTypes);
                return;
            }

            var arguments = new object[] { clientId, calloutId, title, message, isPermanentlyDismissible, anchor,
                    vsCommandGroupId, vsCommandId };
            method.Invoke(tippingService, arguments);
        }
    }

#pragma warning disable CA1715 // Identifiers should have correct prefix
#pragma warning disable CA1040 // Avoid empty interfaces
    [Guid("DCCC6A2B-F300-4DA1-92E1-8BF4A5BCA795")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [TypeIdentifier]
    [ComImport]
    public interface SVsTippingService
    {
    }
#pragma warning restore CA1040 // Avoid empty interfaces
#pragma warning restore CA1715 // Identifiers should have correct prefix
}
