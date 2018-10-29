using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// View model for a pull request review comment.
    /// </summary>
    public interface IPullRequestReviewCommentViewModel : ICommentViewModel
    {
        /// <summary>
        /// Gets a value indicating whether the user can start a new review with this comment.
        /// </summary>
        bool CanStartReview { get; }

        /// <summary>
        /// Gets the caption for the "Commit" button.
        /// </summary>
        /// <remarks>
        /// This will be "Add a single comment" when not in review mode and "Add review comment"
        /// when in review mode.
        /// </remarks>
        string CommitCaption { get; }

        /// <summary>
        /// Gets a value indicating whether this comment is part of a pending pull request review.
        /// </summary>
        bool IsPending { get; }

        /// <summary>
        /// Gets a command which will commit a new comment and start a review.
        /// </summary>
        ReactiveCommand<Unit, Unit> StartReview { get; }

        /// <summary>
        /// Initializes the view model with data.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="review">The associated pull request review.</param>
        /// <param name="comment">The comment model.</param>
        /// <param name="state">The comment edit state.</param>
        Task InitializeAsync(
            IPullRequestSession session,
            ICommentThreadViewModel thread,
            PullRequestReviewModel review,
            PullRequestReviewCommentModel comment,
            CommentEditState state);

        /// <summary>
        /// Initializes the view model as a placeholder.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="isPending">Whether the comment thread is a pending review thread.</param>
        /// <param name="isEditing">Whether to start the placeholder in edit mode.</param>
        Task InitializeAsPlaceholderAsync(
            IPullRequestSession session,
            ICommentThreadViewModel thread,
            bool isPending,
            bool isEditing);
    }
}