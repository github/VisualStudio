using System;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.Services
{
    /// <summary>
    /// A pull request session used to display inline reviews.
    /// </summary>
    /// <remarks>
    /// A pull request session represents the real-time state of a pull request in the IDE.
    /// It takes the pull request model and updates according to the current state of the
    /// repository on disk and in the editor.
    /// </remarks>
    public interface IPullRequestSession
    {
        /// <summary>
        /// Gets a value indicating whether the pull request branch is the currently checked out branch.
        /// </summary>
        bool IsCheckedOut { get; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        IAccount User { get; }

        /// <summary>
        /// Gets the pull request.
        /// </summary>
        IPullRequestModel PullRequest { get; }

        /// <summary>
        /// Gets the pull request's repository.
        /// </summary>
        ILocalRepositoryModel Repository { get; }

        /// <summary>
        /// Adds a new comment to the session.
        /// </summary>
        /// <param name="comment">The comment.</param>
        Task AddComment(IPullRequestReviewCommentModel comment);

        /// <summary>
        /// Gets all files touched by the pull request.
        /// </summary>
        /// <returns>
        /// A list of the files touched by the pull request.
        /// </returns>
        Task<IReactiveList<IPullRequestSessionFile>> GetAllFiles();

        /// <summary>
        /// Gets a file touched by the pull request.
        /// </summary>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <returns>
        /// A <see cref="IPullRequestSessionFile"/> object or null if the file was not touched by
        /// the pull request.
        /// </returns>
        Task<IPullRequestSessionFile> GetFile(string relativePath);

        /// <summary>
        /// Gets a file touched by the pull request.
        /// </summary>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <param name="contentSource">The editor file content source.</param>
        /// <returns>
        /// A <see cref="IPullRequestSessionFile"/> object or null if the file was not touched by
        /// the pull request.
        /// </returns>
        Task<IPullRequestSessionFile> GetFile(
            string relativePath,
            IEditorContentSource contentSource);

        /// <summary>
        /// Converts a path to a path relative to the current repository.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The relative path, or null if the specified path is not in the repository.
        /// </returns>
        string GetRelativePath(string path);

        /// <summary>
        /// Updates the pull request session with a new pull request model in response to a refresh
        /// from the server.
        /// </summary>
        /// <param name="pullRequest">The new pull request model.</param>
        /// <returns>A task which completes when the session has completed updating.</returns>
        Task Update(IPullRequestModel pullRequest);

        /// <summary>
        /// Notifies the session that the contents of a file in the editor have changed.
        /// </summary>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <returns>A task which completes when the session has completed updating.</returns>
        Task UpdateEditorContent(string relativePath);
    }
}
