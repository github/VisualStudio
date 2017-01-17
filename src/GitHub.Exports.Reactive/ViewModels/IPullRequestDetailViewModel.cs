using System;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Describes how changed files are displayed in a the pull request details view.
    /// </summary>
    public enum ChangedFilesViewType
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
    /// Holds immutable state relating to the <see cref="IPullRequestDetailViewModel.Checkout"/> command.
    /// </summary>
    public interface IPullRequestCheckoutState
    {
        /// <summary>
        /// Gets the message to display on the checkout button.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Gets a value indicating whether checkout is available.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets the message to display as the checkout button's tooltip.
        /// </summary>
        string ToolTip { get; }
    }

    /// <summary>
    /// Holds immutable state relating to the <see cref="IPullRequestDetailViewModel.Pull"/> and
    /// <see cref="IPullRequestDetailViewModel.Push"/> commands.
    /// </summary>
    public interface IPullRequestUpdateState
    {
        /// <summary>
        /// Gets the number of commits that the current branch is ahead of the PR branch.
        /// </summary>
        int CommitsAhead { get; }

        /// <summary>
        /// Gets the number of commits that the current branch is behind the PR branch.
        /// </summary>
        int CommitsBehind { get; }

        /// <summary>
        /// Gets a value indicating whether the current branch is up to date.
        /// </summary>
        bool UpToDate { get; }

        /// <summary>
        /// Gets the message to display when a pull cannot be carried out.
        /// </summary>
        string PullToolTip { get; }

        /// <summary>
        /// Gets the message to display when a push cannot be carried out.
        /// </summary>
        string PushToolTip { get; }
    }

    /// <summary>
    /// Represents a view model for displaying details of a pull request.
    /// </summary>
    public interface IPullRequestDetailViewModel : IViewModel, IHasBusy
    {
        /// <summary>
        /// Gets the underlying pull request model.
        /// </summary>
        IPullRequestModel Model { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's source branch.
        /// </summary>
        string SourceBranchDisplayName { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's target branch.
        /// </summary>
        string TargetBranchDisplayName { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request comes from a fork.
        /// </summary>
        bool IsFromFork { get; }

        /// <summary>
        /// Gets the pull request body.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets or sets a value describing how changed files are displayed in a view.
        /// </summary>
        ChangedFilesViewType ChangedFilesViewType { get; set; }

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
        IReactiveList<IPullRequestFileNode> ChangedFilesList { get; }

        /// <summary>
        /// Gets the state associated with the <see cref="Checkout"/> command.
        /// </summary>
        IPullRequestCheckoutState CheckoutState { get; }

        /// <summary>
        /// Gets the state associated with the <see cref="Pull"/> and <see cref="Push"/> commands.
        /// </summary>
        IPullRequestUpdateState UpdateState { get; }

        /// <summary>
        /// Gets the error message to be displayed below the checkout button.
        /// </summary>
        string OperationError { get; }

        /// <summary>
        /// Gets a command that checks out the pull request locally.
        /// </summary>
        ReactiveCommand<Unit> Checkout { get; }

        /// <summary>
        /// Gets a command that pulls changes to the current branch.
        /// </summary>
        ReactiveCommand<Unit> Pull { get; }

        /// <summary>
        /// Gets a command that pushes changes from the current branch.
        /// </summary>
        ReactiveCommand<Unit> Push { get; }

        /// <summary>
        /// Gets a command that opens the pull request on GitHub.
        /// </summary>
        ReactiveCommand<object> OpenOnGitHub { get; }

        /// <summary>
        /// Gets a command that toggles the <see cref="ChangedFilesViewType"/> property.
        /// </summary>
        ReactiveCommand<object> ToggleChangedFilesView { get; }

        /// <summary>
        /// Gets a command that toggles the <see cref="OpenChangedFileAction"/> property.
        /// </summary>
        ReactiveCommand<object> ToggleOpenChangedFileAction { get; }

        /// <summary>
        /// Gets a command that opens a <see cref="IPullRequestFileNode"/>.
        /// </summary>
        ReactiveCommand<object> OpenFile { get; }

        /// <summary>
        /// Gets a command that diffs a <see cref="IPullRequestFileNode"/>.
        /// </summary>
        ReactiveCommand<object> DiffFile { get; }

        /// <summary>
        /// Gets the specified file as it appears in the pull request.
        /// </summary>
        /// <param name="file">The file or directory node.</param>
        /// <returns>The path to the extracted file.</returns>
        Task<string> ExtractFile(IPullRequestFileNode file);

        /// <summary>
        /// Gets the before and after files needed for viewing a diff.
        /// </summary>
        /// <param name="file">The changed file.</param>
        /// <returns>A tuple containing the full path to the before and after files.</returns>
        Task<Tuple<string, string>> ExtractDiffFiles(IPullRequestFileNode file);
    }
}
