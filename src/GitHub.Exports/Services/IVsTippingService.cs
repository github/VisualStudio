using System;
using System.Windows;

namespace GitHub.Services
{
    public interface ITippingService
    {
        void RequestCalloutDisplay(Guid calloutId, string title, string message,
            bool isPermanentlyDismissible, FrameworkElement targetElement, Guid vsCommandGroupId, uint vsCommandId);
    }
}
