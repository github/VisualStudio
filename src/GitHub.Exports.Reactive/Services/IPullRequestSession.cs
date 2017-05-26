using System;
using System.Threading.Tasks;
using GitHub.Models;
using Microsoft.VisualStudio.Text;
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
        /// <param name="snapshot">The current snapshot of the file in an editor.</param>
        /// <returns>
        /// A <see cref="IPullRequestSessionFile"/> object or null if the file was not touched by
        /// the pull request.
        /// </returns>
        Task<IPullRequestSessionFile> GetFile(string relativePath, ITextSnapshot snapshot);

        /// <summary>
        /// Updates the line numbers of the inline comments of a file.
        /// </summary>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <param name="contents">The new file contents.</param>
        /// <returns>A tack which completes when the operation has completed.</returns>
        Task RecaluateLineNumbers(string relativePath, string contents);
    }
}
