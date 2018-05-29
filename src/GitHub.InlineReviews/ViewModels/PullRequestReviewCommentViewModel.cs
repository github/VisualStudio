using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio.UI;
using ReactiveUI;
using Serilog;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// View model for a pull request review comment.
    /// </summary>
    public class PullRequestReviewCommentViewModel : CommentViewModel, IPullRequestReviewCommentViewModel
    {
        readonly IPullRequestSession session;
        ObservableAsPropertyHelper<bool> canStartReview;
        ObservableAsPropertyHelper<string> commitCaption;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReviewCommentViewModel"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="commentId">The REST ID of the comment.</param>
        /// <param name="commentNodeId">The GraphQL ID of the comment.</param>
        /// <param name="body">The comment body.</param>
        /// <param name="state">The comment edit state.</param>
        /// <param name="user">The author of the comment.</param>
        /// <param name="updatedAt">The modified date of the comment.</param>
        /// <param name="isPending">Whether this is a pending comment.</param>
        public PullRequestReviewCommentViewModel(
            IPullRequestSession session,
            ICommentThreadViewModel thread,
            IAccount currentUser,
            int commentId,
            string commentNodeId,
            string body,
            CommentEditState state,
            IAccount user,
            DateTimeOffset updatedAt,
            bool isPending)
            : base(thread, currentUser, commentId, commentNodeId, body, state, user, updatedAt)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            this.session = session;
            IsPending = isPending;

            var pendingReviewAndIdObservable = Observable.CombineLatest(
                session.WhenAnyValue(x => x.HasPendingReview, x => !x),
                this.WhenAnyValue(model => model.Id, i => i == 0),
                (hasPendingReview, isNewComment) => new { hasPendingReview, isNewComment });

            canStartReview = pendingReviewAndIdObservable
                    .Select(arg => arg.hasPendingReview && arg.isNewComment)
                    .ToProperty(this, x => x.CanStartReview);

            commitCaption = pendingReviewAndIdObservable
                .Select(arg => !arg.isNewComment ? Resources.EditComment : arg.hasPendingReview ? Resources.AddSingleComment : Resources.AddReviewComment)
                .ToProperty(this, x => x.CommitCaption);

            StartReview = ReactiveCommand.CreateAsyncTask(
                CommitEdit.CanExecuteObservable,
                DoStartReview);
            AddErrorHandler(StartReview);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReviewCommentViewModel"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="model">The comment model.</param>
        public PullRequestReviewCommentViewModel(
            IPullRequestSession session,
            ICommentThreadViewModel thread,
            IAccount currentUser,
            IPullRequestReviewCommentModel model)
            : this(session, thread, currentUser, model.Id, model.NodeId, model.Body, CommentEditState.None, model.User, model.CreatedAt, model.IsPending)
        {
        }

        /// <summary>
        /// Creates a placeholder comment which can be used to add a new comment to a thread.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="thread">The comment thread.</param>
        /// <param name="currentUser">The current user.</param>
        /// <returns>THe placeholder comment.</returns>
        public static CommentViewModel CreatePlaceholder(
            IPullRequestSession session,
            ICommentThreadViewModel thread,
            IAccount currentUser)
        {
            return new PullRequestReviewCommentViewModel(
                session,
                thread,
                currentUser,
                0,
                null,
                string.Empty,
                CommentEditState.Placeholder,
                currentUser,
                DateTimeOffset.MinValue,
                false);
        }

        /// <inheritdoc/>
        public bool CanStartReview => canStartReview.Value;

        /// <inheritdoc/>
        public string CommitCaption => commitCaption.Value;

        /// <inheritdoc/>
        public bool IsPending { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> StartReview { get; }

        async Task DoStartReview(object unused)
        {
            IsSubmitting = true;

            try
            {
                await session.StartReview();
                await CommitEdit.ExecuteAsync(null);
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}
