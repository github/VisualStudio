using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// Holds the details of a Pull Request.
    /// </summary>
    public class PullRequestDetailModel : IssueishDetailModel
    {
        /// <summary>
        /// Gets or sets the pull request state (open, closed, merged).
        /// </summary>
        public PullRequestStateEnum State { get; set; }

        /// <summary>
        /// Gets or sets the name of the base branch (e.g. "master").
        /// </summary>
        public string BaseRefName { get; set; }

        /// <summary>
        /// Gets or sets the SHA of the base branch.
        /// </summary>
        public string BaseRefSha { get; set; }

        /// <summary>
        /// Gets or sets the owner login of the repository containing the base branch.
        /// </summary>
        public string BaseRepositoryOwner { get; set; }

        /// <summary>
        /// Gets or sets the name of the head branch (e.g. "feature-branch").
        /// </summary>
        public string HeadRefName { get; set; }

        /// <summary>
        /// Gets or sets the SHA of the head branch.
        /// </summary>
        public string HeadRefSha { get; set; }

        /// <summary>
        /// Gets or sets the owner login of the repository containing the head branch.
        /// </summary>
        public string HeadRepositoryOwner { get; set; }

        /// <summary>
        /// Gets or sets a collection of files changed by the pull request.
        /// </summary>
        public IReadOnlyList<PullRequestFileModel> ChangedFiles { get; set; }

        /// <summary>
        /// Gets or sets a collection of pull request reviews.
        /// </summary>
        public IReadOnlyList<PullRequestReviewModel> Reviews { get; set; }

        /// <summary>
        /// Gets or sets a collection of pull request review comment threads.
        /// </summary>
        /// <remarks>
        /// The <see cref="Threads"/> collection groups the comments in the various <see cref="Reviews"/>
        /// into threads, as such each pull request review comment will appear in both collections.
        /// </remarks>
        public IReadOnlyList<PullRequestReviewThreadModel> Threads { get; set; }

        /// <summary>
        /// Gets or sets a collection of pull request Checks Suites
        /// </summary>
        public IReadOnlyList<CheckSuiteModel> CheckSuites { get; set; }

        /// <summary>
        /// Gets or sets a collection of pull request Statuses
        /// </summary>
        public IReadOnlyList<StatusModel> Statuses { get; set; }
    }
}
