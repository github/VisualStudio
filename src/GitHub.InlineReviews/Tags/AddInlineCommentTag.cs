using System;
using GitHub.Services;
using GitHub.Models;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// A tag which marks a line in an editor where a new review comment can be added.
    /// </summary>
    public class AddInlineCommentTag : InlineTag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddInlineCommentTag"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="commitSha">
        /// The SHA of the commit to which a new comment should be added. May be null if the tag
        /// represents trying to add a comment to a line that hasn't yet been pushed.
        /// </param>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="diffLine">The line in the diff that the line relates to.</param>
        /// <param name="lineNumber">The line in the file.</param>
        /// <param name="diffChangeType">The type of represented by the diff line.</param>
        public AddInlineCommentTag(
            IPullRequestSession session,
            string commitSha,
            string filePath,
            int diffLine,
            int lineNumber,
            DiffChangeType diffChangeType)
            : base(session, lineNumber, diffChangeType)
        {
            CommitSha = commitSha;
            DiffLine = diffLine;
            FilePath = filePath;
        }

        /// <summary>
        /// Gets the SHA of the commit to which a new comment should be added.
        /// </summary>
        /// <remarks>
        /// May be null if the tag represents trying to add a comment to a line that hasn't yet been
        /// pushed.
        /// </remarks>
        public string CommitSha { get; }

        /// <summary>
        /// Gets the line in the diff that the line relates to.
        /// </summary>
        public int DiffLine { get; }

        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        public string FilePath { get; }
    }
}
