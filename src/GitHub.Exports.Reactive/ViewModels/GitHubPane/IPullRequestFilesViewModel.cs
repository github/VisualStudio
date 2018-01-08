using System;
using System.Collections.Generic;
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
        ReactiveCommand<object> DiffFile { get; }

        /// <summary>
        /// Gets a command that opens an <see cref="IPullRequestFileNode"/> as it appears in the PR.
        /// </summary>
        ReactiveCommand<object> ViewFile { get; }

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
        /// Initializes the view model.
        /// </summary>
        /// <param name="repositoryPath">The absolute path to the repository.</param>
        /// <param name="session">The pull request session.</param>
        /// <param name="changes">The libgit2sharp representation of the changes.</param>
        /// <param name="commentFilter">An optional review comment filter.</param>
        Task InitializeAsync(
            string repositoryPath,
            IPullRequestSession session,
            TreeChanges changes,
            Func<IInlineCommentThreadModel, bool> commentFilter = null);
    }
}