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
    /// Holds details about a pull request review.
    /// </summary>
    public class PullRequestReviewModel
    {
        /// <summary>
        /// Gets or sets the GraphQL ID of the pull request review.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the author of the pull request review.
        /// </summary>
        public ActorModel Author { get; set; }

        /// <summary>
        /// Gets or sets the review's body markdown.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the review's state (approved, requested changes, commented etc).
        /// </summary>
        public PullRequestReviewState State { get; set; }

        /// <summary>
        /// Gets or sets the SHA at which the review was left.
        /// </summary>
        public string CommitId { get; set; }

        /// <summary>
        /// Gets or sets the date/time at which the review was submitted.
        /// </summary>
        public DateTimeOffset? SubmittedAt { get; set; }

        /// <summary>
        /// Gets or sets the review comments.
        /// </summary>
        public IReadOnlyList<PullRequestReviewCommentModel> Comments { get; set; } = Array.Empty<PullRequestReviewCommentModel>();
    }
}
