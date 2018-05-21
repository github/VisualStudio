using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using ReactiveUI;
using Serilog;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// View model for an issue or pull request comment.
    /// </summary>
    public class CommentViewModel : ReactiveObject, ICommentViewModel
    {
        static readonly ILogger log = LogManager.ForContext<CommentViewModel>();
        string body;
        string errorMessage;
        bool isReadOnly;
        bool isSubmitting;
        CommentEditState state;
        DateTimeOffset updatedAt;
        string undoBody;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="commentId">The ID of the comment.</param>
        /// <param name="commentNodeId">The GraphQL ID of the comment.</param>
        /// <param name="body">The comment body.</param>
        /// <param name="state">The comment edit state.</param>
        /// <param name="user">The author of the comment.</param>
        /// <param name="updatedAt">The modified date of the comment.</param>
        protected CommentViewModel(
            ICommentThreadViewModel thread,
            IAccount currentUser,
            int commentId,
            string commentNodeId,
            string body,
            CommentEditState state,
            IAccount user,
            DateTimeOffset updatedAt)
        {
            Guard.ArgumentNotNull(thread, nameof(thread));
            Guard.ArgumentNotNull(currentUser, nameof(currentUser));
            Guard.ArgumentNotNull(body, nameof(body));
            Guard.ArgumentNotNull(user, nameof(user));

            Thread = thread;
            CurrentUser = currentUser;
            Id = commentId;
            NodeId = commentNodeId;
            Body = body;
            EditState = state;
            User = user;
            UpdatedAt = updatedAt;

            var canDelete = this.WhenAnyValue(
                x => x.EditState,
                x => x == CommentEditState.None && user.Login.Equals(currentUser.Login));

            Delete = ReactiveCommand.Create(canDelete);

            var canCreate = this.WhenAnyValue(
                x => x.EditState,
                x => x == CommentEditState.Placeholder ||
                     (x == CommentEditState.None && user.Login.Equals(currentUser.Login)));

            BeginCreate = ReactiveCommand.Create(canCreate);
            BeginCreate.Subscribe(DoBeginCreate);
            AddErrorHandler(BeginCreate);

            CommitCreate = ReactiveCommand.CreateAsyncTask(
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.IsReadOnly),
                    this.WhenAnyValue(x => x.Body, x => !string.IsNullOrWhiteSpace(x)),
                    this.WhenAnyObservable(x => x.Thread.PostComment.CanExecuteObservable),
                    (readOnly, hasBody, canPost) => !readOnly && hasBody && canPost),
                DoCommitCreate);
            AddErrorHandler(CommitCreate);

            CancelCreate = ReactiveCommand.Create(CommitCreate.IsExecuting.Select(x => !x));
            CancelCreate.Subscribe(DoCancelCreate);
            AddErrorHandler(CancelCreate);

            OpenOnGitHub = ReactiveCommand.Create(this.WhenAnyValue(x => x.Id, x => x != 0));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="model">The comment model.</param>
        protected CommentViewModel(
            ICommentThreadViewModel thread,
            IAccount currentUser,
            ICommentModel model)
            : this(thread, currentUser, model.Id, model.NodeId, model.Body, CommentEditState.None, model.User, model.CreatedAt)
        {
        }

        protected void AddErrorHandler<T>(ReactiveCommand<T> command)
        {
            command.ThrownExceptions.Subscribe(x => ErrorMessage = x.Message);
        }

        void DoBeginCreate(object unused)
        {
            if (state != CommentEditState.Creating)
            {
                undoBody = Body;
                EditState = CommentEditState.Creating;
            }
        }

        void DoCancelCreate(object unused)
        {
            if (EditState == CommentEditState.Creating)
            {
                EditState = string.IsNullOrWhiteSpace(undoBody) ? CommentEditState.Placeholder : CommentEditState.None;
                Body = undoBody;
                ErrorMessage = null;
                undoBody = null;
            }
        }

        async Task DoCommitCreate(object unused)
        {
            try
            {
                ErrorMessage = null;
                IsSubmitting = true;

                var model = await Thread.PostComment.ExecuteAsyncTask(Body);
                Id = model.Id;
                NodeId = model.NodeId;
                EditState = CommentEditState.None;
                UpdatedAt = DateTimeOffset.Now;
            }
            catch (Exception e)
            {
                var message = e.Message;
                ErrorMessage = message;
                log.Error(e, "Error posting comment");
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        /// <inheritdoc/>
        public int Id { get; private set; }

        /// <inheritdoc/>
        public string NodeId { get; private set; }

        /// <inheritdoc/>
        public string Body
        {
            get { return body; }
            set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <inheritdoc/>
        public string ErrorMessage
        {
            get { return this.errorMessage; }
            private set { this.RaiseAndSetIfChanged(ref errorMessage, value); }
        }

        /// <inheritdoc/>
        public CommentEditState EditState
        {
            get { return state; }
            private set { this.RaiseAndSetIfChanged(ref state, value); }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { this.RaiseAndSetIfChanged(ref isReadOnly, value); }
        }

        /// <inheritdoc/>
        public bool IsSubmitting
        {
            get { return isSubmitting; }
            protected set { this.RaiseAndSetIfChanged(ref isSubmitting, value); }
        }

        /// <inheritdoc/>
        public DateTimeOffset UpdatedAt
        {
            get { return updatedAt; }
            private set { this.RaiseAndSetIfChanged(ref updatedAt, value); }
        }

        /// <inheritdoc/>
        public IAccount CurrentUser { get; }

        /// <inheritdoc/>
        public ICommentThreadViewModel Thread { get; }

        /// <inheritdoc/>
        public IAccount User { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> BeginCreate { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> CancelCreate { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> CommitCreate { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> OpenOnGitHub { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> Delete { get; }
    }
}
