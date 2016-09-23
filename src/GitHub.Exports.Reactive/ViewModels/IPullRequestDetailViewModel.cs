using System;
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
