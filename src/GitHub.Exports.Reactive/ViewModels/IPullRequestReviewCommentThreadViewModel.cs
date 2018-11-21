using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A thread of pull request review comments.
    /// </summary>
    public interface IPullRequestReviewCommentThreadViewModel : ICommentThreadViewModel
    {
        /// <summary>
        /// Gets the current pull request review session.
        /// </summary>
        IPullRequestSession Session { get; }

        /// <summary>
        /// Gets the file that the comment is on.
        /// </summary>
        IPullRequestSessionFile File { get; }

        /// <summary>
        /// Gets the 0-based line number that the comment in on.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the side of the diff that the comment is on.
        /// </summary>
        DiffSide Side { get; }

        /// <summary>
        /// Gets a value indicating whether the thread is a new thread being authored, that is not
        /// yet present on the server.
        /// </summary>
        bool IsNewThread { get; }

        /// <summary>
        /// Gets a value indicating whether the user must commit and push their changes before
        /// leaving a comment on the requested line.
        /// </summary>
        bool NeedsPush { get; }

        /// <summary>
        /// Initializes the view model with data.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="file">The file that the comment is on.</param>
        /// <param name="thread">The thread.</param>
        /// <param name="addPlaceholder">
        ///     Whether to add a placeholder comment at the end of the thread.
        /// </param>
        Task InitializeAsync(IPullRequestSession session,
            IPullRequestSessionFile file,
            IInlineCommentThreadModel thread,
            bool addPlaceholder);

        /// <summary>
        /// Initializes the view model as a new thread being authored.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="file">The file that the comment is on.</param>
        /// <param name="lineNumber">The 0-based line number of the thread.</param>
        /// <param name="side">The side of the diff.</param>
        /// <param name="isEditing">Whether to start the placeholder in edit state.</param>
        Task InitializeNewAsync(IPullRequestSession session,
            IPullRequestSessionFile file,
            int lineNumber,
            DiffSide side,
            bool isEditing);
    }
}
