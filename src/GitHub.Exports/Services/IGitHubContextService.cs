using System;
using System.Threading.Tasks;

namespace GitHub.Services
{
    /// <summary>
    /// Methods for constructing a <see cref="GitHubContext"/> and navigating based on a <see cref="GitHubContext"/>.
    /// </summary>
    public interface IGitHubContextService
    {
        /// <summary>
        /// Find the context from a URL in the clipboard if any.
        /// </summary>
        /// <returns>The context or null if clipboard doesn't contain a GitHub URL</returns>
        GitHubContext FindContextFromClipboard();

        /// <summary>
        /// Find the context from the title of the topmost browser.
        /// </summary>
        /// <returns>A context or null if a context can't be found.</returns>
        GitHubContext FindContextFromBrowser();

        /// <summary>
        /// Convert a GitHub URL to a context object.
        /// </summary>
        /// <param name="url">A GitHub URL</param>
        /// <returns>The context from the URL or null</returns>
        GitHubContext FindContextFromUrl(string url);

        /// <summary>
        /// Convert a context to a repository URL.
        /// </summary>
        /// <param name="context">The context to convert.</param>
        /// <returns>A repository URL</returns>
        GitHubContext FindContextFromWindowTitle(string windowTitle);

        /// <summary>
        /// Find a context from a browser window title.
        /// </summary>
        /// <param name="windowTitle">A browser window title.</param>
        /// <returns>The context or null if none can be found</returns>
        Uri ToRepositoryUrl(GitHubContext context);

        /// <summary>
        /// Open a file in the working directory that corresponds to a context and navigate to a line/range.
        /// </summary>
        /// <param name="repositoryDir">The working directory.</param>
        /// <param name="context">A context to navigate to.</param>
        /// <returns>True if navigation was successful</returns>
        bool TryOpenFile(string repositoryDir, GitHubContext context);

        /// <summary>
        /// Attempt to open the Blame/Annotate view for a context.
        /// </summary>
        /// <remarks>
        /// The access to the Blame/Annotate view was added in a version of Visual Studio 2017. This method will return
        /// false is this functionality isn't available.
        /// </remarks>
        /// <param name="repositoryDir">The target repository</param>
        /// <param name="currentBranch">A branch in the local repository. It isn't displayed on the UI but must exist. It can be a remote or local branch.</param>
        /// <param name="context">The context to open.</param>
        /// <returns>True if AnnotateFile functionality is available.</returns>
        Task<bool> TryAnnotateFile(string repositoryDir, string currentBranch, GitHubContext context);

        /// <summary>
        /// Map from a context to a repository blob object.
        /// </summary>
        /// <param name="repositoryDir">The target repository.</param>
        /// <param name="context">The context to map from.</param>
        /// <param name="remoteName">The name of the remote to search for branches.</param>
        /// <returns>The resolved commit-ish, blob path and commit SHA for the blob. Path will be null if the commit-ish can be resolved but not the blob.</returns>
        (string commitish, string path, string commitSha) ResolveBlob(string repositoryDir, GitHubContext context, string remoteName = "origin");

        /// <summary>
        /// Check if a file in the working directory has changed since a specified commit-ish.
        /// </summary>
        /// <remarks>
        /// The commit-ish might be a commit SHA, a tag or a remote branch.
        /// </remarks>
        /// <param name="repositoryDir">The target repository.</param>
        /// <param name="commitish">A commit SHA, remote branch or tag.</param>
        /// <param name="path">The path for a blob.</param>
        /// <returns>True if the working file is different.</returns>
        bool HasChangesInWorkingDirectory(string repositoryDir, string commitish, string path);
    }
}