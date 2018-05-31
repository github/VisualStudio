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

    public class PullRequestReviewModel
    {
        public string Id { get; set; }
        public ActorModel Author { get; set; }
        public string Body { get; set; }
        public PullRequestReviewState State { get; set; }
        public string CommitId { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public IReadOnlyList<PullRequestReviewCommentModel> Comments { get; set; } = Array.Empty<PullRequestReviewCommentModel>();
    }
}
