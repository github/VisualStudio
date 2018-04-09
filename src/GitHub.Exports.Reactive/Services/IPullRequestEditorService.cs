using System.Threading.Tasks;
using GitHub.Models;
using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.Services
{
    /// <summary>
    /// Services for opening views of pull request files in Visual Studio.
    /// </summary>
    public interface IPullRequestEditorService
    {
        /// <summary>
        /// Opens an editor for a file in a pull request.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="workingDirectory">
        /// If true opens the file in the working directory, if false opens the file in the HEAD
        /// commit of the pull request.
        /// </param>
        /// <returns>A task tracking the operation.</returns>
        Task OpenFile(IPullRequestSession session, string relativePath, bool workingDirectory);

        /// <summary>
        /// Opens an diff viewer for a file in a pull request.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="headSha">
        /// The commit SHA of the right hand side of the diff. Pass null to compare with the
        /// working directory, or "HEAD" to compare with the HEAD commit of the pull request.
        /// </param>
        /// <returns>A task tracking the operation.</returns>
        Task OpenDiff(IPullRequestSession session, string relativePath, string headSha = null);

        /// <summary>
        /// Opens an diff viewer for a file in a pull request with the specified inline comment
        /// thread open.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="thread">The thread to open</param>
        /// <returns>A task tracking the operation.</returns>
        Task OpenDiff(IPullRequestSession session, string relativePath, IInlineCommentThreadModel thread);

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