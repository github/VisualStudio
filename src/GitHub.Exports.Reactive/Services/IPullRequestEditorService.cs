using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Services for opening views of pull request files in Visual Studio.
    /// </summary>
    public interface IPullRequestEditorService
    {
        /// <summary>
        /// Opens an editor for a file in a pull request.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="workingDirectory">
        /// If true opens the file in the working directory, if false opens the file in the HEAD
        /// commit of the pull request.
        /// </param>
        /// <returns>A task tracking the operation.</returns>
        Task OpenFile(IPullRequestSession session, string relativePath, bool workingDirectory);

        /// <summary>
        /// Opens an diff viewer for a file in a pull request.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="workingDirectory">
        /// If true the right hand side of the diff will be the current state of the file in the
        /// working directory, if false it will be the HEAD commit of the pull request.
        /// </param>
        /// <returns>A task tracking the operation.</returns>
        Task OpenDiff(IPullRequestSession session, string relativePath, bool workingDirectory);

        /// <summary>
        /// Opens an diff viewer for a file in a pull request with the specified inline comment
        /// thread open.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <param name="thread">The thread to open</param>
        /// <returns>A task tracking the operation.</returns>
        Task OpenDiff(IPullRequestSession session, string relativePath, IInlineCommentThreadModel thread);
    }
}