using System;
using Octokit;

namespace GitHub.Models
{
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
    }
}
