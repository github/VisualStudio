using System;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    /// <summary>
    /// Provides services for interacting with issues.
    /// </summary>
    public interface IIssueService
    {
        /// <summary>
        /// Reads a page of issue items.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="after">The end cursor of the previous page, or null for the first page.</param>
        /// <param name="states">The issue states to filter by</param>
        /// <returns>A page of issue item models.</returns>
        Task<Page<IssueListItemModel>> ReadIssues(
            HostAddress address,
            string owner,
            string name,
            string after,
            IssueState[] states);

        /// <summary>
        /// Reads the details of a specified issue.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="number">The issue number.</param>
        /// <returns>A task returning the issue model.</returns>
        Task<IssueDetailModel> ReadIssue(
            HostAddress address,
            string owner,
            string name,
            int number);

        /// <summary>
        /// Reads a page of users that can be assigned to issues.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="after">The end cursor of the previous page, or null for the first page.</param>
        /// <returns>A page of author models.</returns>
        Task<Page<ActorModel>> ReadAssignableUsers(
            HostAddress address,
            string owner,
            string name,
            string after);
    }
}
