using System;
using System.Windows;

namespace GitHub.Services
{
    /// <summary>
    /// This service is a thin wrapper around <see cref="Microsoft.Internal.VisualStudio.Shell.Interop.IVsTippingService"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IVsTippingService"/> interface is public, but contained within the 'Microsoft.VisualStudio.Shell.UI.Internal' assembly.
    /// To avoid a direct dependency on 'Microsoft.VisualStudio.Shell.UI.Internal', we use reflection to call this service.
    /// </remarks>
    public interface ITippingService
    {
        /// <summary>
        /// Show a call-out notification with the option to execute a command.
        /// </summary>
        /// <param name="calloutId">A unique id for the callout so that is can be permanently dismissed.</param>
        /// <param name="title">A clickable title for the callout.</param>
        /// <param name="message">A plain text message for that callout that will automatically wrap.</param>
        /// <param name="isPermanentlyDismissible">True for an option to never show again.</param>
        /// <param name="targetElement">A UI element for the callout to appear above which must be visible.</param>
        /// <param name="vsCommandGroupId">The group of the command to execute when title is clicked.</param>
        /// <param name="vsCommandId">The ID of the command to execute when title is clicked.</param>
        void RequestCalloutDisplay(Guid calloutId, string title, string message,
            bool isPermanentlyDismissible, FrameworkElement targetElement, Guid vsCommandGroupId, uint vsCommandId);
    }
}
