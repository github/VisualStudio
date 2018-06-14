using System;
using System.Collections.Generic;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents an item in the pull request list.
    /// </summary>
    public interface IPullRequestListItemViewModel : IViewModel
    {
        /// <summary>
        /// Gets the ID of the issue or pull request.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the author of the issue or pull request.
        /// </summary>
        IActorViewModel Author { get; }

        /// <summary>
        /// Gets the number of comments in the issue or pull request.
        /// </summary>
        int CommentCount { get; }

        /// <summary>
        /// Gets the labels applied to the issue or pull request.
        /// </summary>
        IReadOnlyList<ILabelViewModel> Labels { get; }

        /// <summary>
        /// Gets the issue or pull request number.
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets the issue or pull request title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the last updated time of the issue or pull request.
        /// </summary>
        DateTimeOffset UpdatedAt { get; }
    }
}
