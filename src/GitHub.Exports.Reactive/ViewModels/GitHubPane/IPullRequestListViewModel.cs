using System;
using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model which displays a pull request list.
    /// </summary>
    public interface IPullRequestListViewModel : IIssueListViewModelBase, IOpenInBrowser
    {
        /// <summary>
        /// Gets a command which navigates to the "Create Pull Request" view.
        /// </summary>
        ReactiveCommand<Unit, Unit> CreatePullRequest { get; }

        /// <summary>
        /// Gets a command that opens the pull request conversation in a document pane.
        /// </summary>
        ReactiveCommand<IPullRequestListItemViewModel, Unit> OpenConversation { get; }

        /// <summary>
        /// Gets a command that opens the pull request item on GitHub.
        /// </summary>
        ReactiveCommand<IPullRequestListItemViewModel, IPullRequestListItemViewModel> OpenItemInBrowser { get; }
    }
}
