using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// The possible states of a pull request review.
    /// </summary>
    public enum PullRequestReviewState
    {
        /// <summary>
        /// A review that has not yet been submitted.
        /// </summary>
        Pending,

        /// <summary>
        /// An informational review.
        /// </summary>
        Commented,

        /// <summary>
        /// A review allowing the pull request to merge.
        /// </summary>
        Approved,

        /// <summary>
        /// A review blocking the pull request from merging.
        /// </summary>
        ChangesRequested,

        /// <summary>
        /// A review that has been dismissed.
        /// </summary>
        Dismissed,
    }

    /// <summary>
    /// Represents a review of a pull request.
    /// </summary>
    public interface IPullRequestReviewModel
    {
        /// <summary>
        /// Gets the ID of the review.
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Gets the GraphQL ID for the review.
        /// </summary>
        string NodeId { get; set; }

        /// <summary>
        /// Gets the author of the review.
        /// </summary>
        IAccount User { get; }

        /// <summary>
        /// Gets the body of the review.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the state of the review.
        /// </summary>
        PullRequestReviewState State { get; }

        /// <summary>
        /// Gets the SHA of the commit that the review was submitted on.
        /// </summary>
        string CommitId { get; }

        /// <summary>
        /// Gets the date/time that the review was submitted.
        /// </summary>
        DateTimeOffset? SubmittedAt { get; }

        /// <summary>
        /// Gets the comments for the review.
        /// </summary>
        IReadOnlyList<IPullRequestReviewCommentModel> Comments { get; }
    }
}
