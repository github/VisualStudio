using System;
using System.Reactive;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
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
        ReactiveCommand<Unit> StartReview { get; }
    }
}