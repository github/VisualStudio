using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.UI.Helpers;

namespace GitHub.VisualStudio.UI.Helpers
{
    /// <summary>
    /// Utilities for fixing WPF's broken ScrollViewer.
    /// </summary>
    public static class ScrollViewerUtilities
    {
        /// <summary>
        /// Fixes mouse wheel scrolling in controls that have a ScrollViewer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// WPF's ScrollViewer is broken in that it doesn't pass scroll events to the parent
        /// control when it can't scroll any more. Add this method as a PreviewMouseWheel event
        /// handler to a ScrollViewer or a control which has a ScrollViewer in its template to
        /// fix this.
        /// </remarks>
        public static void FixMouseWheelScroll(object sender, MouseWheelEventArgs e)
        {
            try
            {
                if (!e.Handled)
                {
                    var control = sender as FrameworkElement;
                    var parent = control.Parent as UIElement;
                    var scrollViewer = control.GetSelfAndVisualDescendents()
                        .OfType<ScrollViewer>()
                        .FirstOrDefault();

                    if (scrollViewer != null && parent != null)
                    {
                        var offset = scrollViewer.ContentVerticalOffset;

                        if ((offset == scrollViewer.ScrollableHeight && e.Delta < 0) ||
                            (offset == 0 && e.Delta > 0))
                        {
                            e.Handled = true;
                            parent.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                            {
                                RoutedEvent = UIElement.MouseWheelEvent,
                                Source = control,
                            });
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
