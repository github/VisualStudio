using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels
{
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
    public interface IPullRequestDetailViewModel : IViewModel, IHasLoading, IHasBusy, IHasErrorState
    {
        /// <summary>
        /// Gets the underlying pull request model.
        /// </summary>
        IPullRequestModel Model { get; }

        /// <summary>
        /// Gets the session for the pull request.
        /// </summary>
        IPullRequestSession Session { get; }

        /// <summary>
        /// Gets the local repository.
        /// </summary>
        ILocalRepositoryModel LocalRepository { get; }

        /// <summary>
        /// Gets the owner of the remote repository that contains the pull request.
        /// </summary>
        /// <remarks>
        /// The remote repository may be different from the local repository if the local
        /// repository is a fork and the user is viewing pull requests from the parent repository.
        /// </remarks>
        string RemoteRepositoryOwner { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's source branch.
        /// </summary>
        string SourceBranchDisplayName { get; }

        /// <summary>
        /// Gets a string describing how to display the pull request's target branch.
        /// </summary>
        string TargetBranchDisplayName { get; }

        /// <summary>
        /// Gets the number of comments made on the pull request.
        /// </summary>
        int CommentCount { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request branch is checked out.
        /// </summary>
        bool IsCheckedOut { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request comes from a fork.
        /// </summary>
        bool IsFromFork { get; }

        /// <summary>
        /// Gets the pull request body.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the changed files as a tree.
        /// </summary>
        IReadOnlyList<IPullRequestChangeNode> ChangedFilesTree { get; }

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
        /// Gets a command that diffs an <see cref="IPullRequestFileNode"/> between BASE and HEAD.
        /// </summary>
        ReactiveCommand<object> DiffFile { get; }

        /// <summary>
        /// Gets a command that diffs an <see cref="IPullRequestFileNode"/> between the version in
        /// the working directory and HEAD.
        /// </summary>
        ReactiveCommand<object> DiffFileWithWorkingDirectory { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> from disk.
        /// </summary>
        ReactiveCommand<object> OpenFileInWorkingDirectory { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> as it appears in the PR.
        /// </summary>
        ReactiveCommand<object> ViewFile { get; }

        /// <summary>
        /// Gets a file as it appears in the pull request.
        /// </summary>
        /// <param name="file">The changed file.</param>
        /// <param name="head">
        /// If true, gets the file at the PR head, otherwise gets the file at the PR merge base.
        /// </param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>The path to a temporary file.</returns>
        Task<string> ExtractFile(IPullRequestFileNode file, bool head, Encoding encoding);

        /// <summary>
        /// Gets the encoding for the specified file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The file's encoding</returns>
        Encoding GetEncoding(string path);

        /// <summary>
        /// Gets the full path to a file in the working directory.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The full path to the file in the working directory.</returns>
        string GetLocalFilePath(IPullRequestFileNode file);
    }
}
