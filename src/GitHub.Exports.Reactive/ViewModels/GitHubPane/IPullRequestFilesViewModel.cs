using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a tree of changed files in a pull request.
    /// </summary>
    public interface IPullRequestFilesViewModel : IViewModel, IDisposable
    {
        /// <summary>
        /// Gets the number of changed files in the pull request.
        /// </summary>
        int ChangedFilesCount { get; }

        /// <summary>
        /// Gets the root nodes of the tree.
        /// </summary>
        IReadOnlyList<IPullRequestChangeNode> Items { get; }

        /// <summary>
        /// Gets a command that diffs an <see cref="IPullRequestFileNode"/> between BASE and HEAD.
        /// </summary>
        ReactiveCommand<IPullRequestFileNode, Unit> DiffFile { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> as it appears in the PR.
        /// </summary>
        ReactiveCommand<IPullRequestFileNode, Unit> ViewFile { get; }

        /// <summary>
        /// Gets a command that diffs an <see cref="IPullRequestFileNode"/> between the version in
        /// the working directory and HEAD.
        /// </summary>
        ReactiveCommand<IPullRequestFileNode, Unit> DiffFileWithWorkingDirectory { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> from disk.
        /// </summary>
        ReactiveCommand<IPullRequestFileNode, Unit> OpenFileInWorkingDirectory { get; }

        /// <summary>
        /// Gets a command that opens the first comment for a <see cref="IPullRequestFileNode"/> in
        /// the diff viewer.
        /// </summary>
        ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstComment { get; }

        /// <summary>
        /// Gets a command that opens the first annotation notice for a <see cref="IPullRequestFileNode"/> in
        /// the diff viewer.
        /// </summary>
        ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationNotice { get; }

        /// <summary>
        /// Gets a command that opens the first annotation warning for a <see cref="IPullRequestFileNode"/> in
        /// the diff viewer.
        /// </summary>
        ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationWarning { get; }

        /// <summary>
        /// Gets a command that opens the first annotation failure for a <see cref="IPullRequestFileNode"/> in
        /// the diff viewer.
        /// </summary>
        ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationFailure { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="commentFilter">An optional review comment filter.</param>
        Task InitializeAsync(
            IPullRequestSession session,
            Func<IInlineCommentThreadModel, bool> commentFilter = null);
    }
}