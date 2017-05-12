using System;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Octokit;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// A thread of inline comments (aka Pull Request Review Comments).
    /// </summary>
    class InlineCommentThreadViewModel : CommentThreadViewModel
    {
        readonly IApiClient apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use to post/update comments.</param>
        /// <param name="session">The current PR review session.</param>
        /// <param name="commitSha">The SHA of the commit that the thread relates to.</param>
        /// <param name="filePath">The path to the file that the thread relates to.</param>
        /// <param name="diffLine">The line in the diff that the thread relates to.</param>
        public InlineCommentThreadViewModel(
            IApiClient apiClient,
            IPullRequestSession session,
            string commitSha,
            string filePath,
            int diffLine)
            : base(session.User)
        {
            Guard.ArgumentNotNull(apiClient, nameof(apiClient));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(commitSha, nameof(commitSha));
            Guard.ArgumentNotNull(filePath, nameof(filePath));

            this.apiClient = apiClient;
            Session = session;
            CommitSha = commitSha;
            DiffLine = diffLine;
            FilePath = filePath;
        }

        /// <summary>
        /// Gets the SHA of the commit that the thread relates to.
        /// </summary>
        public string CommitSha { get; }

        /// <summary>
        /// Gets line in the diff between the PR base and <see cref="CommitSha"/> that the thread
        /// relates to.
        /// </summary>
        public int DiffLine { get; }

        /// <summary>
        /// Gets the path to the file that the thread relates to.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the current pull request review session.
        /// </summary>
        public IPullRequestSession Session { get; }

        /// <inheritdoc/>
        public override async Task<ICommentModel> PostComment(string body)
        {
            Guard.ArgumentNotNull(body, nameof(body));

            var lastComment = Comments.Where(x => x.Id != 0).LastOrDefault();
            var result = lastComment != null ?
                await PostReply(body, lastComment.Id) :
                await PostNewComment(body);

            var model = new PullRequestReviewCommentModel
            {
                Body = result.Body,
                CommitId = result.CommitId,
                DiffHunk = result.DiffHunk,
                Id = result.Id,
                OriginalCommitId = result.OriginalCommitId,
                OriginalPosition = result.OriginalPosition,
                Path = result.Path,
                Position = result.Position,
                UpdatedAt = result.UpdatedAt,
                User = Session.User,
            };

            ////Session.AddComment(model);
            return model;
        }

        protected override ICommentViewModel CreateReplyPlaceholder()
        {
            return CommentViewModel.CreatePlaceholder(this, CurrentUser);
        }

        Task<PullRequestReviewComment> PostNewComment(string body)
        {
            return apiClient.CreatePullRequestReviewComment(
                Session.Repository.Owner,
                Session.Repository.Name,
                Session.PullRequest.Number,
                body,
                CommitSha,
                FilePath.Replace("\\", "/"),
                DiffLine + 1).ToTask();
        }

        Task<PullRequestReviewComment> PostReply(string body, int replyTo)
        {
            return apiClient.CreatePullRequestReviewComment(
                Session.Repository.Owner,
                Session.Repository.Name,
                Session.PullRequest.Number,
                body,
                replyTo).ToTask();
        }
    }
}
