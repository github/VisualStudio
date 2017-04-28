using System.Windows;
using System.Windows.Input;

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
        /// control when it can't scroll any more. Add this method as an event handler to a
        /// control which has a ScrollViewer in its template to fix this.
        /// </remarks>
        public static void FixMouseWheelScroll(object sender, MouseWheelEventArgs e)
        {
            try
            {
                if (!e.Handled)
                {
                    var control = sender as FrameworkElement;
                    var parent = control.Parent as UIElement;

                    if (parent != null)
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
            catch
            {
            }
        }
    }
}
