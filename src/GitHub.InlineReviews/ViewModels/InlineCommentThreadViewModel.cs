using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Octokit;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// A thread of inline comments (aka Pull Request Review Comments).
    /// </summary>
    public class InlineCommentThreadViewModel : CommentThreadViewModel
    {
        readonly IApiClient apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use to post/update comments.</param>
        /// <param name="session">The current PR review session.</param>
        public InlineCommentThreadViewModel(
            IApiClient apiClient,
            IPullRequestSession session,
            IEnumerable<IPullRequestReviewCommentModel> comments)
            : base(session.User)
        {
            Guard.ArgumentNotNull(apiClient, nameof(apiClient));
            Guard.ArgumentNotNull(session, nameof(session));

            this.apiClient = apiClient;
            Session = session;
            PostComment = ReactiveCommand.CreateAsyncTask(
                Observable.Return(true),
                DoPostComment);

            foreach (var comment in comments)
            {
                Comments.Add(new CommentViewModel(this, CurrentUser, comment));
            }

            Comments.Add(CommentViewModel.CreatePlaceholder(this, CurrentUser));
        }

        /// <summary>
        /// Gets the current pull request review session.
        /// </summary>
        public IPullRequestSession Session { get; }

        async Task<ICommentModel> DoPostComment(object parameter)
        {
            Guard.ArgumentNotNull(parameter, nameof(parameter));

            var body = (string)parameter;
            var replyId = Comments[0].Id;
            var result = await apiClient.CreatePullRequestReviewComment(
                Session.RepositoryOwner,
                Session.LocalRepository.Name,
                Session.PullRequest.Number,
                body,
                replyId);

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
                CreatedAt = result.CreatedAt,
                User = Session.User,
            };

            await Session.AddComment(model);
            return model;
        }
    }
}
