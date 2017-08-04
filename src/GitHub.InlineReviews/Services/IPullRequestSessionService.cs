using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Provides a common interface for services required by <see cref="PullRequestSession"/>.
    /// </summary>
    public interface IPullRequestSessionService
    {
        /// <summary>
        /// Carries out a diff between a file at a commit and the current file contents.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="baseSha">The commit to use as the base.</param>
        /// <param name="headSha">The commit to use as the head.</param>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <param name="contents">The contents of the file.</param>
        /// <returns></returns>
        Task<IList<DiffChunk>> Diff(
            ILocalRepositoryModel repository,
            string baseSha,
            string headSha,
            string relativePath,
            byte[] contents);

        /// <summary>
        /// Tests whether the contents of a file represent a commit that is pushed to origin.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <param name="contents">The contents of the file.</param>
        /// <returns>
        /// A task returning true if the file is unmodified with respect to the latest commit
        /// pushed to origin; otherwise false.
        /// </returns>
        Task<bool> IsUnmodifiedAndPushed(
            ILocalRepositoryModel repository,
            string relativePath,
            byte[] contents);

        /// <summary>
        /// Extracts a file at a specified commit from the repository.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="commitSha">The SHA of the commit.</param>
        /// <param name="relativePath">The path to the file, relative to the repository.</param>
        /// <returns>
        /// The contents of the file, or null if the file was not found at the specified commit.
        /// </returns>
        Task<byte[]> ExtractFileFromGit(
            ILocalRepositoryModel repository,
            int pullRequestNumber,
            string sha,
            string relativePath);

        /// <summary>
        /// Gets the SHA of the tip of the current branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>The tip SHA.</returns>
        Task<string> GetTipSha(ILocalRepositoryModel repository);

        /// <summary>
        /// Asynchronously reads the contents of a file.
        /// </summary>
        /// <param name="path">The full path to the file.</param>
        /// <returns>
        /// A task returning the contents of the file, or null if the file was not found.
        /// </returns>
        Task<byte[]> ReadFileAsync(string path);

        /// <summary>
        /// Find the merge base for a pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request.</param>
        /// <returns>
        /// The merge base SHA for the PR.
        /// </returns>
        Task<string> GetPullRequestMergeBase(ILocalRepositoryModel repository, IPullRequestModel pullRequest);
    }
}
