using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// View model for a pull request review comment.
    /// </summary>
    [Export(typeof(IPullRequestReviewCommentViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestReviewCommentViewModel : CommentViewModel, IPullRequestReviewCommentViewModel
    {
        readonly ObservableAsPropertyHelper<bool> canStartReview;
        IPullRequestSession session;
        bool isPending;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReviewCommentViewModel"/> class.
        /// </summary>
        /// <param name="commentService">The comment service</param>
        [ImportingConstructor]
        public PullRequestReviewCommentViewModel(ICommentService commentService)
            : base(commentService)
        {
            canStartReview = this.WhenAnyValue(
                x => x.IsPending,
                x => x.Id,
                (isPending, id) => !isPending && id == null)
                .ToProperty(this, x => x.CanStartReview);

            StartReview = ReactiveCommand.CreateFromTask(DoStartReview, CommitEdit.CanExecute);
            AddErrorHandler(StartReview);
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            IPullRequestSession session,
            ICommentThreadViewModel thread,
            PullRequestReviewModel review,
            PullRequestReviewCommentModel comment,
            CommentEditState state)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            await InitializeAsync(thread, session.User, comment, state).ConfigureAwait(true);
            this.session = session;
            IsPending = review.State == PullRequestReviewState.Pending;
        }

        /// <inheritdoc/>
        public async Task InitializeAsPlaceholderAsync(
            IPullRequestSession session,
            ICommentThreadViewModel thread,
            bool isPending,
            bool isEditing)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            await InitializeAsync(
                thread, 
                session.User,
                null,
                isEditing ? CommentEditState.Editing : CommentEditState.Placeholder).ConfigureAwait(true);
            this.session = session;
            IsPending = isPending;
        }

        /// <inheritdoc/>
        public bool CanStartReview => canStartReview.Value;

        /// <inheritdoc/>
        public bool IsPending
        {
            get => isPending;
            private set => this.RaiseAndSetIfChanged(ref isPending, value);
        }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> StartReview { get; }

        protected override IObservable<string> GetCommitCaptionObservable()
        {
            return this.WhenAnyValue(
                x => x.IsPending,
                x => x.Id,
                (pending, id) => id != null ?
                    Resources.UpdateComment :
                    pending ? Resources.AddReviewComment : Resources.AddSingleComment);
        }

        async Task DoStartReview()
        {
            IsSubmitting = true;

            try
            {
                await session.StartReview().ConfigureAwait(true);
                await CommitEdit.Execute();
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}
