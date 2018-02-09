using System.Windows;

namespace GitHub.InlineReviews.Services
{
    public interface IPullRequestStatusBarManager
    {
        /// <summary>
        /// Place the PR status control on Visual Studio's status bar.
        /// </summary>
        /// <param name="mainWindow">The main window of Visual Studio.</param>
        void Initialize(Window mainWindow);
    }
}
