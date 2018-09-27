using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
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
        ICommentService commentService;
        string body;
        string errorMessage;
        bool isReadOnly;
        bool isSubmitting;
        CommentEditState state;
        DateTimeOffset updatedAt;
        string undoBody;
        ObservableAsPropertyHelper<bool> canDelete;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="commentService">The comment service</param>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="pullRequestId">The pull request id of the comment.</param>
        /// <param name="commentId">The GraphQL ID of the comment.</param>
        /// <param name="databaseId">The database id of the comment.</param>
        /// <param name="body">The comment body.</param>
        /// <param name="state">The comment edit state.</param>
        /// <param name="author">The author of the comment.</param>
        /// <param name="updatedAt">The modified date of the comment.</param>
        /// <param name="webUrl"></param>
        public CommentViewModel(
            ICommentService commentService,
            ICommentThreadViewModel thread,
            IActorViewModel currentUser,
            int pullRequestId,
            string commentId,
            int databaseId,
            string body,
            CommentEditState state,
            IActorViewModel author,
            DateTimeOffset updatedAt,
            Uri webUrl)
        {
            this.commentService = commentService;
            Guard.ArgumentNotNull(thread, nameof(thread));
            Guard.ArgumentNotNull(currentUser, nameof(currentUser));
            Guard.ArgumentNotNull(author, nameof(author));

            Thread = thread;
            CurrentUser = currentUser;
            Id = commentId;
            DatabaseId = databaseId;
            PullRequestId = pullRequestId;
            Body = body;
            EditState = state;
            Author = author;
            UpdatedAt = updatedAt;
            WebUrl = webUrl;

            var canDeleteObservable = this.WhenAnyValue(
                x => x.EditState,
                x => x == CommentEditState.None && author.Login == currentUser.Login);

            canDelete = canDeleteObservable.ToProperty(this, x => x.CanDelete);

            Delete = ReactiveCommand.CreateAsyncTask(canDeleteObservable, DoDelete);

            var canEdit = this.WhenAnyValue(
                x => x.EditState,
                x => x == CommentEditState.Placeholder || (x == CommentEditState.None && author.Login == currentUser.Login));

            BeginEdit = ReactiveCommand.Create(canEdit);
            BeginEdit.Subscribe(DoBeginEdit);
            AddErrorHandler(BeginEdit);

            CommitEdit = ReactiveCommand.CreateAsyncTask(
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.IsReadOnly),
                    this.WhenAnyValue(x => x.Body, x => !string.IsNullOrWhiteSpace(x)),
                    this.WhenAnyObservable(x => x.Thread.PostComment.CanExecuteObservable),
                    (readOnly, hasBody, canPost) => !readOnly && hasBody && canPost),
                DoCommitEdit);
            AddErrorHandler(CommitEdit);

            CancelEdit = ReactiveCommand.Create(CommitEdit.IsExecuting.Select(x => !x));
            CancelEdit.Subscribe(DoCancelEdit);
            AddErrorHandler(CancelEdit);

            OpenOnGitHub = ReactiveCommand.Create(this.WhenAnyValue(x => x.Id).Select(x => x != null));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="commentService">Comment Service</param>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="model">The comment model.</param>
        public CommentViewModel(
            ICommentService commentService,
            ICommentThreadViewModel thread,
            ActorModel currentUser,
            CommentModel model)
            : this(
                  commentService,
                  thread, 
                  new ActorViewModel(currentUser),
                  model.PullRequestId, 
                  model.Id, 
                  model.DatabaseId, 
                  model.Body, 
                  CommentEditState.None, 
                  new ActorViewModel(model.Author), 
                  model.CreatedAt,
                  new Uri(model.Url))
        {
        }

        protected void AddErrorHandler<T>(ReactiveCommand<T> command)
        {
            command.ThrownExceptions.Subscribe(x => ErrorMessage = x.Message);
        }

        async Task DoDelete(object unused)
        {
            if (commentService.ConfirmCommentDelete())
            {
                try
                {
                    ErrorMessage = null;
                    IsSubmitting = true;

                    await Thread.DeleteComment.ExecuteAsyncTask(new Tuple<int, int>(PullRequestId, DatabaseId));
                }
                catch (Exception e)
                {
                    var message = e.Message;
                    ErrorMessage = message;
                    log.Error(e, "Error Deleting comment");
                }
                finally
                {
                    IsSubmitting = false;
                }
            }
        }

        void DoBeginEdit(object unused)
        {
            if (state != CommentEditState.Editing)
            {
                ErrorMessage = null;
                undoBody = Body;
                EditState = CommentEditState.Editing;
            }
        }

        void DoCancelEdit(object unused)
        {
            if (EditState == CommentEditState.Editing)
            {
                EditState = string.IsNullOrWhiteSpace(undoBody) ? CommentEditState.Placeholder : CommentEditState.None;
                Body = undoBody;
                ErrorMessage = null;
                undoBody = null;
            }
        }

        async Task DoCommitEdit(object unused)
        {
            try
            {
                ErrorMessage = null;
                IsSubmitting = true;

                if (Id == null)
                {
                    await Thread.PostComment.ExecuteAsyncTask(Body);
                }
                else
                {
                    await Thread.EditComment.ExecuteAsyncTask(new Tuple<string, string>(Id, Body));
                }
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
        public string Id { get; private set; }

        /// <inheritdoc/>
        public int DatabaseId { get; private set; }
    
        /// <inheritdoc/>
        public int PullRequestId { get; private set; }

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

        public bool CanDelete
        {
            get { return canDelete.Value; }
        }

        /// <inheritdoc/>
        public DateTimeOffset UpdatedAt
        {
            get { return updatedAt; }
            private set { this.RaiseAndSetIfChanged(ref updatedAt, value); }
        }

        /// <inheritdoc/>
        public IActorViewModel CurrentUser { get; }

        /// <inheritdoc/>
        public ICommentThreadViewModel Thread { get; }

        /// <inheritdoc/>
        public IActorViewModel Author { get; }

        /// <inheritdoc/>
        public Uri WebUrl { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> BeginEdit { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> CancelEdit { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> CommitEdit { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> OpenOnGitHub { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> Delete { get; }
    }
}
