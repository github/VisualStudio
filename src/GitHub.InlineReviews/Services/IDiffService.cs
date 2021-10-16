using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using LibGit2Sharp;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Service for generating parsed diffs.
    /// </summary>
    public interface IDiffService
    {
        /// <summary>
        /// Calculates the diff of a file in a repository between two commits.
        /// </summary>
        /// <param name="repo">The repository</param>
        /// <param name="baseSha">The base commit SHA.</param>
        /// <param name="headSha">The head commit SHA.</param>
        /// <param name="relativePath">The path to the file in the repository.</param>
        /// <returns>
        /// A collection of <see cref="DiffChunk"/>s containing the parsed diff.
        /// </returns>
        Task<IReadOnlyList<DiffChunk>> Diff(IRepository repo, string baseSha, string headSha, string relativePath);

        /// <summary>
        /// Calculates the diff of a file in a repository between a base commit and a byte arrat.
        /// </summary>
        /// <param name="repo">The repository</param>
        /// <param name="baseSha">The base commit SHA.</param>
        /// <param name="headSha">The head commit SHA.</param>
        /// <param name="relativePath">The path to the file in the repository.</param>
        /// <param name="contents">The byte array to compare with the base SHA.</param>
        /// <returns>
        /// A collection of <see cref="DiffChunk"/>s containing the parsed diff.
        /// </returns>
        /// <remarks>
        /// Note that even though the comparison is done between <paramref name="baseSha"/> and
        /// <paramref name="contents"/>, the <paramref name="headSha"/> still needs to be provided in order
        /// to track renames.
        /// </remarks>
        Task<IReadOnlyList<DiffChunk>> Diff(IRepository repo, string baseSha, string headSha, string relativePath, byte[] contents);
    }
}