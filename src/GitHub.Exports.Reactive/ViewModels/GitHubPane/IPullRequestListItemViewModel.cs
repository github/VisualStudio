using System;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents an item in the pull request list.
    /// </summary>
    public interface IPullRequestListItemViewModel : IIssueListItemViewModelBase
    {
        /// <summary>
        /// Gets the ID of the pull request.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the number of comments in the pull request.
        /// </summary>
        int CommentCount { get; }

        /// <summary>
        /// Gets a value indicating whether the currently checked out branch is the pull request
        /// branch.
        /// </summary>
        bool IsCurrent { get; }

        /// <summary>
        /// Gets the last updated time of the pull request.
        /// </summary>
        DateTimeOffset UpdatedAt { get; }

        /// <summary>
        /// Gets the pull request checks and statuses summary
        /// </summary>
        PullRequestChecksEnum Checks { get; }
    }
}
