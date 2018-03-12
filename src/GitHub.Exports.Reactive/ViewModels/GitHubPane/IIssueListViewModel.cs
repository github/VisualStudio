using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    [Flags]
    public enum IssueStateFilter
    {
        Open,
        Closed,
        All,
    }

    /// <summary>
    /// <summary>
    /// Represents a view model for displaying the issues for a repository.
    /// </summary>
    public interface IIssueListViewModel : ISearchablePageViewModel, IOpenInBrowser
    {
        /// <summary>
        /// Gets the list of issues.
        /// </summary>
        IReadOnlyList<IIssueListItemViewModel> Issues { get; }

        /// <summary>
        /// Gets a list of the open/closed state filters.
        /// </summary>
        IReadOnlyList<IssueStateFilter> States { get; }

        /// <summary>
        /// Gets or sets the selected open/closed state.
        /// </summary>
        IssueStateFilter SelectedState { get; set; }

        /// <summary>
        /// Gets the author filter.
        /// </summary>
        IUserFilterViewModel AuthorFilter { get; }

        /// <summary>
        /// Gets the assignee filter.
        /// </summary>
        IUserFilterViewModel AssigneeFilter { get; }

        /// <summary>
        /// Gets or sets the selected issue assignee filter.
        /// </summary>
        string SelectedAssignee { get; set; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="connection">The connection to the repository host.</param>
        Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);
    }
}
