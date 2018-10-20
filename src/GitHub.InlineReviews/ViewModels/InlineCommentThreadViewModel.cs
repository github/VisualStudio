using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Services;
using GitHub.Models;
using GitHub.Services;
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
        /// <param name="commentService">The comment service</param>
        /// <param name="session">The current PR review session.</param>
        /// <param name="comments">The comments to display in this inline review.</param>
        public InlineCommentThreadViewModel(ICommentService commentService, IPullRequestSession session,
            IEnumerable<InlineCommentModel> comments)
            : base(session.User)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            Session = session;

            PostComment = ReactiveCommand.CreateFromTask<string>(DoPostComment);
            EditComment = ReactiveCommand.CreateFromTask<Tuple<string, string>>(DoEditComment);
            DeleteComment = ReactiveCommand.CreateFromTask<Tuple<int, int>>(DoDeleteComment);

            foreach (var comment in comments)
            {
                Comments.Add(new PullRequestReviewCommentViewModel(
                    session,
                    commentService,
                    this,
                    CurrentUser,
                    comment.Review,
                    comment.Comment));
            }

            Comments.Add(PullRequestReviewCommentViewModel.CreatePlaceholder(session, commentService, this, CurrentUser));
        }

        /// <summary>
        /// Gets the current pull request review session.
        /// </summary>
        public IPullRequestSession Session { get; }

        async Task DoPostComment(string body)
        {
            Guard.ArgumentNotNull(body, nameof(body));

            var replyId = Comments[0].Id;
            await Session.PostReviewComment(body, replyId);
        }

        async Task DoEditComment(Tuple<string, string> idAndBody)
        {
            Guard.ArgumentNotNull(idAndBody, nameof(idAndBody));

            await Session.EditComment(idAndBody.Item1, idAndBody.Item2);
        }

        async Task DoDeleteComment(Tuple<int, int> item)
        {
            Guard.ArgumentNotNull(item, nameof(item));

            await Session.DeleteComment(item.Item1, item.Item2);
        }
    }
}
