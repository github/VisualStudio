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
    }
}
