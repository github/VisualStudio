using System.Threading.Tasks;
using GitHub.Models;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Differencing;
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
        /// <returns>The opened file.</returns>
        Task<ITextView> OpenFile(IPullRequestSession session, string relativePath, bool workingDirectory);

        /// <summary>
        /// Opens an diff viewer for a file in a pull request.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="headSha">
        /// The commit SHA of the right hand side of the diff. Pass null to compare with the
        /// working directory, or "HEAD" to compare with the HEAD commit of the pull request.
        /// </param>
        /// <param name="scrollToFirstDiff">True to scroll to first difference in file. Set to false if caret is being moved after opening.</param>
        /// <returns>The opened diff viewer.</returns>
        Task<IDifferenceViewer> OpenDiff(IPullRequestSession session, string relativePath, string headSha = null, bool scrollToFirstDiff = true);

        /// <summary>
        /// Opens an diff viewer for a file in a pull request with the specified inline review
        /// comment thread open.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="thread">The thread to open</param>
        /// <returns>The opened diff viewer.</returns>
        Task<IDifferenceViewer> OpenDiff(IPullRequestSession session, string relativePath, IInlineCommentThreadModel thread);

        /// <summary>
        /// Opens an diff viewer for a file in a pull request with the specified inline review line open.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="headSha">
        /// The commit SHA of the right hand side of the diff. Pass null to compare with the
        /// working directory, or "HEAD" to compare with the HEAD commit of the pull request.
        /// </param>
        /// <param name="nextInlineTagFromLine">The 0-based line number to execute NextInlineCommentCommand from</param>
        /// <returns>The opened diff viewer.</returns>
        Task<IDifferenceViewer> OpenDiff(IPullRequestSession session, string relativePath, string headSha, int nextInlineTagFromLine);

        /// <summary>
        /// Find the active text view.
        /// </summary>
        /// <returns>The active view or null if view can't be found.</returns>
        IVsTextView FindActiveView();

        /// <summary>
        /// Place the caret at the best guess equivalent position in <see cref="targetView"/>.
        /// </summary>
        /// <param name="sourceView">The text view to navigate from.</param>
        /// <param name="targetView">The text view to navigate to.</param>
        void NavigateToEquivalentPosition(IVsTextView sourceView, IVsTextView targetView);

        /// <summary>
        /// Check to see if text view is an ediatable document.
        /// </summary>
        /// <param name="textView">The text view to check.</param>
        /// <returns>True if text view is editable and part of a diff view.</returns>
        bool IsEditableDiff(ITextView textView);

        /// <summary>
        /// Open the active document in a new code view.
        /// </summary>
        /// <remarks>
        /// If the active document is part of a diff view, open in a new code view.
        /// </remarks>
        /// <param name="sourceView">The source view to use the line and position from.</param>
        void OpenActiveDocumentInCodeView(IVsTextView sourceView);
    }
}