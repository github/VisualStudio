using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="InlineCommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use to post/update comments.</param>
        /// <param name="session">The current PR review session.</param>
        public InlineCommentThreadViewModel(
            IPullRequestSession session,
            IEnumerable<IPullRequestReviewCommentModel> comments)
            : base(session.User)
        {
            Guard.ArgumentNotNull(session, nameof(session));

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

        /// <inheritdoc/>
        public override Uri GetCommentUrl(int id)
        {
            return new Uri(string.Format(
                CultureInfo.InvariantCulture,
                "{0}/pull/{1}#discussion_r{2}",
                Session.LocalRepository.CloneUrl.ToRepositoryUrl(Session.RepositoryOwner),
                Session.PullRequest.Number,
                id));
        }

        async Task<ICommentModel> DoPostComment(object parameter)
        {
            Guard.ArgumentNotNull(parameter, nameof(parameter));

            var body = (string)parameter;
            var replyId = Comments[0].Id;
            var nodeId = Comments[0].NodeId;
            return await Session.PostReviewComment(body, replyId, nodeId);
        }
    }
}
