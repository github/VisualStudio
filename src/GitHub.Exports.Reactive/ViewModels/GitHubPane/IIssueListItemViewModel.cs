using System;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents an item in the issue list.
    /// </summary>
    public interface IIssueListItemViewModel : IIssueListItemViewModelBase
    {
        /// <summary>
        /// Gets the ID of the issue.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the number of comments in the issue.
        /// </summary>
        int CommentCount { get; }

        /// <summary>
        /// Gets the last updated time of the issue.
        /// </summary>
        DateTimeOffset UpdatedAt { get; }
    }
}
