using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    /// <summary>
    /// Services for issues and pull requests.
    /// </summary>
    public interface IIssueishService
    {
        /// <summary>
        /// Closes an issue or pull request.
        /// </summary>
        /// <param name="address">The address of the server.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repository">The repository name.</param>
        /// <param name="number">The issue or pull request number.</param>
        Task CloseIssueish(HostAddress address, string owner, string repository, int number);

        /// <summary>
        /// Reopens an issue or pull request.
        /// </summary>
        /// <param name="address">The address of the server.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repository">The repository name.</param>
        /// <param name="number">The issue or pull request number.</param>
        Task ReopenIssueish(HostAddress address, string owner, string repository, int number);

        /// <summary>
        /// Posts an issue or pull request comment.
        /// </summary>
        /// <param name="address">The address of the server.</param>
        /// <param name="issueishId">The GraphQL ID of the issue or pull request.</param>
        /// <param name="body">The comment body.</param>
        /// <returns>The model for the comment that was added.</returns>
        Task<CommentModel> PostComment(
            HostAddress address,
            string issueishId,
            string body);

        /// <summary>
        /// Deletes an issue or pull request comment.
        /// </summary>
        /// <param name="address">The address of the server.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repository">The repository name.</param>
        /// <param name="commentId">The database ID of the comment.</param>
        Task DeleteComment(
            HostAddress address,
            string owner,
            string repository,
            int commentId);

        /// <summary>
        /// Edits an issue or pull request comment.
        /// </summary>
        /// <param name="address">The address of the server.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="repository">The repository name.</param>
        /// <param name="commentId">The database ID of the comment.</param>
        /// <param name="body">The new comment body.</param>
        Task EditComment(
            HostAddress address,
            string owner,
            string repository,
            int commentId,
            string body);
    }
}
