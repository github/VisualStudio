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
        /// Gets the root nodes of the tree.
        /// </summary>
        IReadOnlyList<IPullRequestChangeNode> Items { get; }

        /// <summary>
        /// Gets a command that diffs an <see cref="IPullRequestFileNode"/> between BASE and HEAD.
        /// </summary>
        ReactiveCommand<Unit> DiffFile { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> as it appears in the PR.
        /// </summary>
        ReactiveCommand<Unit> ViewFile { get; }

        /// <summary>
        /// Gets a command that diffs an <see cref="IPullRequestFileNode"/> between the version in
        /// the working directory and HEAD.
        /// </summary>
        ReactiveCommand<Unit> DiffFileWithWorkingDirectory { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> from disk.
        /// </summary>
        ReactiveCommand<Unit> OpenFileInWorkingDirectory { get; }

        /// <summary>
        /// Gets a command that opens the first comment for a <see cref="IPullRequestFileNode"/> in
        /// the diff viewer.
        /// </summary>
        ReactiveCommand<Unit> OpenFirstComment { get; }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="changes">The libgit2sharp representation of the changes.</param>
        /// <param name="commentFilter">An optional review comment filter.</param>
        Task InitializeAsync(
            IPullRequestSession session,
            TreeChanges changes,
            Func<IInlineCommentThreadModel, bool> commentFilter = null);
    }
}