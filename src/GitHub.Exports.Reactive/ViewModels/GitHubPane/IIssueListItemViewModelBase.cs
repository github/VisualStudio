using System;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Base interface for items in a issue or pull request list.
    /// </summary>
    public interface IIssueListItemViewModelBase : IViewModel
    {
        /// <summary>
        /// Gets the issue or pull request number.
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets the issue or pull request title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the author of the issue or pull request.
        /// </summary>
        IActorViewModel Author { get; }
    }
}
