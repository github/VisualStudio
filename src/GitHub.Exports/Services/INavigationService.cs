using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.Services
{
    public interface INavigationService
    {
        /// <summary>
        /// Find the active text view.
        /// </summary>
        /// <returns>The active view or null if view can't be found.</returns>
        IVsTextView FindActiveView();

        /// <summary>
        /// Navigate to and place the caret at the best guess equivalent position in <see cref="targetFile"/>.
        /// </summary>
        /// <param name="sourceView">The text view to navigate from.</param>
        /// <param name="targetFile">The text view to open and navigate to.</param>
        /// <returns>The opened text view.</returns>
        IVsTextView NavigateToEquivalentPosition(IVsTextView sourceView, string targetFile);
    }
}