using System;
using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Describes how changed files are displayed in a the pull request details view.
    /// </summary>
    public enum ChangedFilesView
    {
        /// <summary>
        /// The files are displayed as a tree.
        /// </summary>
        TreeView,

        /// <summary>
        /// The files are displayed as a flat list.
        /// </summary>
        ListView,
    }

    /// <summary>
    /// Describes how files are opened in a the pull request details view.
    /// </summary>
    public enum OpenChangedFileAction
    {
        /// <summary>
        /// Opening the file opens a diff.
        /// </summary>
        Diff,

        /// <summary>
        /// Opening the file opens the file in a text editor.
        /// </summary>
        Open
    }

    /// <summary>
    /// Describes how a pull request will be checked out.
    /// </summary>
    public enum CheckoutMode
    {
        /// <summary>
        /// The pull request branch is checked out and up-to-date.
        /// </summary>
        UpToDate,

        /// <summary>
        /// The pull request branch is checked out but needs updating.
        /// </summary>
        NeedsPull,

        /// <summary>
        /// A local branch exists for the pull request but it is not the current branch.
        /// </summary>
        Switch,

        /// <summary>
        /// No local branch exists for the pull request.
        /// </summary>
        Fetch,

        /// <summary>
        /// The checked out branch is marked as the branch for the pull request, but there are
        /// local commits or the branch shares no history.
        /// </summary>
        InvalidState,
    }

    /// <summary>
    /// Represents a view model for displaying details of a pull request.
    /// </summary>
    public interface IPullRequestDetailViewModel : IViewModel, IHasBusy
    {
        /// <summary>
        /// Gets the state of the pull request, e.g. Open, Closed, Merged.
        /// </summary>
        PullRequestState State { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's source branch.
        /// </summary>
        string SourceBranchDisplayName { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's target branch.
        /// </summary>
        string TargetBranchDisplayName { get; }

        /// <summary>
        /// Gets the number of commits in the pull request.
        /// </summary>
        int CommitCount { get; }

        /// <summary>
        /// Gets the pull request number.
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets the account that submitted the pull request.
        /// </summary>
        IAccount Author { get; }

        /// <summary>
        /// Gets the date and time at which the pull request was created.
        /// </summary>
        DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets the pull request body.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the number of files that have been changed in the pull request.
        /// </summary>
        int ChangedFilesCount { get; }

        /// <summary>
        /// Gets or sets a value describing how changed files are displayed in a view.
        /// </summary>
        ChangedFilesView ChangedFilesView { get; set; }

        /// <summary>
        /// Gets or sets a value describing how files are opened when double clicked.
        /// </summary>
        OpenChangedFileAction OpenChangedFileAction { get; set; }

        /// <summary>
        /// Gets the changed files as a tree.
        /// </summary>
        IReactiveList<IPullRequestChangeNode> ChangedFilesTree { get; }

        /// <summary>
        /// Gets the changed files as a flat list.
        /// </summary>
        IReactiveList<IPullRequestFileViewModel> ChangedFilesList { get; }

        /// <summary>
        /// Gets the checkout mode for the pull request.
        /// </summary>
        CheckoutMode CheckoutMode { get; }

        /// <summary>
        /// Gets the error message to be displayed below the checkout button.
        /// </summary>
        string CheckoutError { get; }

        /// <summary>
        /// Gets the number of commits that the current branch is behind the PR when <see cref="CheckoutMode"/>
        /// is <see cref="CheckoutMode.NeedsPull"/>.
        /// </summary>
        int CommitsBehind { get; }

        /// <summary>
        /// Gets a message indicating the why the <see cref="Checkout"/> command is disabled.
        /// </summary>
        string CheckoutDisabledMessage { get; }

        /// <summary>
        /// Gets a command that checks out the pull request locally.
        /// </summary>
        ReactiveCommand<Unit> Checkout { get; }

        /// <summary>
        /// Gets a command that opens the pull request on GitHub.
        /// </summary>
        ReactiveCommand<object> OpenOnGitHub { get; }

        /// <summary>
        /// Gets a command that toggles the <see cref="ChangedFilesView"/> property.
        /// </summary>
        ReactiveCommand<object> ToggleChangedFilesView { get; }

        /// <summary>
        /// Gets a command that toggles the <see cref="OpenChangedFileAction"/> property.
        /// </summary>
        ReactiveCommand<object> ToggleOpenChangedFileAction { get; }
    }
}
