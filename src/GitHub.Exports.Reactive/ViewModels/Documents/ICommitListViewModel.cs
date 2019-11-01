using System;
using System.Collections.Generic;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// Displays a list of commit summaries in a pull request timeline.
    /// </summary>
    public interface ICommitListViewModel : IViewModel
    {
        /// <summary>
        /// Gets the first author of the commits in the list.
        /// </summary>
        ICommitActorViewModel Author { get; }

        /// <summary>
        /// Gets a string to display the author login or the author name.
        /// </summary>
        string AuthorName { get; }

        /// <summary>
        /// Gets a string to display next to the author in the view.
        /// </summary>
        string AuthorCaption { get; }

        /// <summary>
        /// Gets the commits.
        /// </summary>
        IReadOnlyList<ICommitSummaryViewModel> Commits { get; }
    }
}