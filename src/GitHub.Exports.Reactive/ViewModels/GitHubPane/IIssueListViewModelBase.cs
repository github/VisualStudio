using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Describes a message that should be displayed in place of a list of items in
    /// an <see cref="IIssueListItemViewModelBase"/>.
    /// </summary>
    public enum IssueListMessage
    {
        /// <summary>
        /// No message should be displayed; the items should be displayed.
        /// </summary>
        None,

        /// <summary>
        /// A "No Open Items" message should be displayed.
        /// </summary>
        NoOpenItems,

        /// <summary>
        /// A "No Items Match Search Criteria" message should be displayed.
        /// </summary>
        NoItemsMatchCriteria,
    }

    /// <summary>
    /// Represents a view model which displays an issue or pull request list.
    /// </summary>
    public interface IIssueListViewModelBase : ISearchablePageViewModel
    {
        /// <summary>
        /// Gets the filter view model.
        /// </summary>
        IUserFilterViewModel AuthorFilter { get; }

        /// <summary>
        /// Gets a list consisting of the fork and parent repositories if the current repository is
        /// a fork.
        /// </summary>
        /// <remarks>
        /// Returns null if the current repository is not a fork.
        /// </remarks>
        IReadOnlyList<RepositoryModel> Forks { get; }

        /// <summary>
        /// Gets the list of issues or pull requests.
        /// </summary>
        IReadOnlyList<IIssueListItemViewModelBase> Items { get; }

        /// <summary>
        /// Gets a filtered view of <see cref="Items"/> based on <see cref="SearchQuery"/> and
        /// <see cref="AuthorFilter"/>.
        /// </summary>
        ICollectionView ItemsView { get; }

        /// <summary>
        /// Gets the local repository.
        /// </summary>
        LocalRepositoryModel LocalRepository { get; }

        /// <summary>
        /// Gets an enum indicating a message that should be displayed in place of a list of items.
        /// </summary>
        IssueListMessage Message { get; }

        /// <summary>
        /// Gets the remote repository.
        /// </summary>
        /// <remarks>
        /// This may differ from <see cref="LocalRepository"/> if <see cref="LocalRepository"/> is
        /// a fork.
        /// </remarks>
        RepositoryModel RemoteRepository { get; set; }

        /// <summary>
        /// Gets the currently selected item in <see cref="States"/>.
        /// </summary>
        string SelectedState { get; set; }

        /// <summary>
        /// Gets a list of the available states (e.g. Open, Closed, All).
        /// </summary>
        IReadOnlyList<string> States { get; }

        /// <summary>
        /// Gets the caption to display as the header on the <see cref="States"/> dropdown.
        /// </summary>
        string StateCaption { get; }

        /// <summary>
        /// Gets a command which opens the item passed as a parameter.
        /// </summary>
        ReactiveCommand<IIssueListItemViewModelBase, Unit> OpenItem { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="repository">The local repository.</param>
        /// <param name="connection">The connection/</param>
        /// <returns>A task tracking the operation.</returns>
        Task InitializeAsync(LocalRepositoryModel repository, IConnection connection);
    }
}
