using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using Octokit;

namespace GitHub.Services
{
    /// <summary>
    /// A pull request session used to display inline comments.
    /// </summary>
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
        /// Gets an observable that indicates that<see cref="PullRequest"/> has been updated.
        /// </summary>
        /// <remarks>
        /// This notification is different to listening for a PropertyChanged event because the
        /// pull request model may be updated in-place which will not result in a PropertyChanged
        /// notification.
        /// </remarks>
        IObservable<IPullRequestModel> PullRequestChanged { get; }

        /// <summary>
        /// Gets the local repository.
        /// </summary>
        ILocalRepositoryModel LocalRepository { get; }

        /// <summary>
        /// Gets the owner of the repository that contains the pull request.
        /// </summary>
        /// <remarks>
        /// If the pull request is targeting <see cref="LocalRepository"/> then the owner will be
        /// the owner of the local repository. If however the pull request targets a different fork
        /// then this property describes the owner of the fork.
        /// </remarks>
        string RepositoryOwner { get; }

        /// <summary>
        /// Gets a value indicating whether the pull request has a pending review for the current
        /// user.
        /// </summary>
        bool HasPendingReview { get; }

        /// <summary>
        /// Gets the ID of the current pending pull request review for the user.
        /// </summary>
        long PendingReviewId { get; }

        /// <summary>
        /// Gets all files touched by the pull request.
        /// </summary>
        /// <returns>
        /// A list of the files touched by the pull request.
        /// </returns>
        Task<IReadOnlyList<IPullRequestSessionFile>> GetAllFiles();

        /// <summary>
        /// Gets a file touched by the pull request.
        /// </summary>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <param name="commitSha">
        /// The commit at which to get the file contents, or "HEAD" to track the pull request head.
        /// </param>
        /// <returns>
        /// A <see cref="IPullRequestSessionFile"/> object or null if the file was not touched by
        /// the pull request.
        /// </returns>
        Task<IPullRequestSessionFile> GetFile(string relativePath, string commitSha = "HEAD");

        /// <summary>
        /// Gets the merge base SHA for the pull request.
        /// </summary>
        /// <returns>The merge base SHA.</returns>
        Task<string> GetMergeBase();

        /// <summary>
        /// Posts a new PR review comment.
        /// </summary>
        /// <param name="body">The comment body.</param>
        /// <param name="commitId">THe SHA of the commit to comment on.</param>
        /// <param name="path">The relative path of the file to comment on.</param>
        /// <param name="fileDiff">The diff between the PR head and base.</param>
        /// <param name="position">The line index in the diff to comment on.</param>
        /// <returns>A comment model.</returns>
        Task<IPullRequestReviewCommentModel> PostReviewComment(
            string body,
            string commitId,
            string path,
            IReadOnlyList<DiffChunk> fileDiff,
            int position);

        /// <summary>
        /// Posts a PR review comment reply.
        /// </summary>
        /// <param name="body">The comment body.</param>
        /// <param name="inReplyTo">The REST ID of the comment to reply to.</param>
        /// <param name="inReplyToNodeId">The GraphQL ID of the comment to reply to.</param>
        /// <returns></returns>
        Task<IPullRequestReviewCommentModel> PostReviewComment(
            string body,
            int inReplyTo,
            string inReplyToNodeId);

        /// <summary>
        /// Starts a new pending pull request review.
        /// </summary>
        Task<IPullRequestReviewModel> StartReview();

        /// <summary>
        /// Cancels the currently pending review.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// There is no pending review.
        /// </exception>
        Task CancelReview();

        /// <summary>
        /// Posts the currently pending review.
        /// </summary>
        /// <param name="body">The review body.</param>
        /// <param name="state">The review event.</param>
        /// <returns>The review model.</returns>
        Task<IPullRequestReviewModel> PostReview(string body, PullRequestReviewEvent e);

        /// <summary>
        /// Updates the pull request session with a new pull request model in response to a refresh
        /// from the server.
        /// </summary>
        /// <param name="pullRequest">The new pull request model.</param>
        /// <returns>A task which completes when the session has completed updating.</returns>
        Task Update(IPullRequestModel pullRequest);
    }
}
