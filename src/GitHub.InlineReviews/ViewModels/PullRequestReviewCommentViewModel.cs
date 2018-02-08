using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using Serilog;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// View model for a pull request review comment.
    /// </summary>
    public class PullRequestReviewCommentViewModel : CommentViewModel, IPullRequestReviewCommentViewModel
    {
        static readonly ILogger log = LogManager.ForContext<CommentViewModel>();
        readonly IPullRequestSession session;
        ObservableAsPropertyHelper<bool> canStartReview;
        ObservableAsPropertyHelper<string> commitCaption;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReviewCommentViewModel"/> class.
        /// </summary>
        /// <param name="session">The pull request session.</param>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="commentId">The ID of the comment.</param>
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
            string body,
            CommentEditState state,
            IAccount user,
            DateTimeOffset updatedAt,
            bool isPending)
            : base(thread, currentUser, commentId, body, state, user, updatedAt)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            this.session = session;
            IsPending = isPending;

            canStartReview = session.WhenAnyValue(x => x.HasPendingReview)
                .Select(x => !x)
                .ToProperty(this, x => x.CanStartReview);
            commitCaption = session.WhenAnyValue(x => x.HasPendingReview)
                .Select(x => x ? "Add review comment" : "Add a single comment")
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
            : this(session, thread, currentUser, model.Id, model.Body, CommentEditState.None, model.User, model.CreatedAt, model.IsPending)
        {
        }

        /// <summary>
        /// Creates a placeholder comment which can be used to add a new comment to a thread.
        /// </summary>
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
            await session.StartReview();
            await CommitEdit.ExecuteAsync(null);
        }
    }
}
