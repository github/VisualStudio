using System;
using GitHub.Extensions;
using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// View model for an inline comment (aka Pull Request Review Comment).
    /// </summary>
    public class InlineCommentViewModel : CommentViewModel, IInlineCommentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="commentId">The ID of the comment.</param>
        /// <param name="body">The comment body.</param>
        /// <param name="state">The comment edit state.</param>
        /// <param name="user">The author of the comment.</param>
        /// <param name="updatedAt">The modified date of the comment.</param>
        public InlineCommentViewModel(
            ICommentThreadViewModel thread,
            IAccount currentUser,
            int commentId,
            string body,
            CommentEditState state,
            IAccount user,
            DateTimeOffset updatedAt,
            string commitSha,
            int diffLine)
            : base(thread, currentUser, commentId, body, state, user, updatedAt)
        {
            Guard.ArgumentNotNull(commitSha, nameof(commitSha));

            CommitSha = commitSha;
            DiffLine = diffLine;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="model">The comment model.</param>
        public InlineCommentViewModel(
            ICommentThreadViewModel thread,
            IAccount currentUser,
            IPullRequestReviewCommentModel model)
            : base(thread, currentUser, model)
        {
            CommitSha = model.OriginalCommitId;
            DiffLine = model.OriginalPosition.Value;
        }

        /// <inheritdoc/>
        public string CommitSha { get; }

        /// <inheritdoc/>
        public int DiffLine { get; }
    }
}
