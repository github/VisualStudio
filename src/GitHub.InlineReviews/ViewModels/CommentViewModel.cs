using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.UI;
using Octokit;
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
        CommentEditState state;
        DateTimeOffset updatedAt;
        string undoBody;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="commentId">The ID of the comment.</param>
        /// <param name="body">The comment body.</param>
        /// <param name="state">The comment edit state.</param>
        /// <param name="user">The author of the comment.</param>
        /// <param name="updatedAt">The modified date of the comment.</param>
        public CommentViewModel(
            ICommentThreadViewModel thread,
            IAccount currentUser,
            int commentId,
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
            Body = body;
            EditState = state;
            User = user;
            UpdatedAt = updatedAt;

            var canEdit = this.WhenAnyValue(
                x => x.EditState,
                x => x == CommentEditState.Placeholder || (x == CommentEditState.None && user.Equals(currentUser)));

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

            OpenOnGitHub = ReactiveCommand.Create(this.WhenAnyValue(x => x.Id, x => x != 0));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="model">The comment model.</param>
        public CommentViewModel(
            ICommentThreadViewModel thread,
            IAccount currentUser,
            ICommentModel model)
            : this(thread, currentUser, model.Id, model.Body, CommentEditState.None, model.User, model.CreatedAt)
        {
        }

        /// <summary>
        /// Creates a placeholder comment which can be used to add a new comment to a thread.
        /// </summary>
        /// <param name="thread">The comment thread.</param>
        /// <param name="currentUser">The current user.</param>
        /// <returns>THe placeholder comment.</returns>
        public static CommentViewModel CreatePlaceholder(
            ICommentThreadViewModel thread,
            IAccount currentUser)
        {
            return new CommentViewModel(
                thread,
                currentUser,
                0,
                string.Empty,
                CommentEditState.Placeholder,
                currentUser,
                DateTimeOffset.MinValue);
        }

        void AddErrorHandler<T>(ReactiveCommand<T> command)
        {
            command.ThrownExceptions.Subscribe(x => ErrorMessage = x.Message);
        }

        void DoBeginEdit(object unused)
        {
            if (state != CommentEditState.Editing)
            {
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
                Id = (await Thread.PostComment.ExecuteAsyncTask(Body)).Id;
                EditState = CommentEditState.None;
                UpdatedAt = DateTimeOffset.Now;
            }
            catch (Exception e)
            {
                var message = e.Message;

                if (e is ApiValidationException)
                {
                    // HACK: If the user has pending review comments on the server then we can't
                    // post new comments. The correct way to test for this would be to make a
                    // request to /repos/:owner/:repo/pulls/:number/reviews and check for comments
                    // with a PENDING state. For the moment however we'll just display a message.
                    message += ". Do you have pending review comments?";
                }

                ErrorMessage = message;
                log.Error(e, "Error posting inline comment");
            }
        }

        /// <inheritdoc/>
        public int Id { get; private set; }

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
        public DateTimeOffset UpdatedAt
        {
            get { return updatedAt; }
            private set { this.RaiseAndSetIfChanged(ref updatedAt, value); }
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public IAccount CurrentUser { get; }

        /// <summary>
        /// Gets the thread that the comment is a part of.
        /// </summary>
        public ICommentThreadViewModel Thread { get; }

        /// <inheritdoc/>
        public IAccount User { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> BeginEdit { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> CancelEdit { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> CommitEdit { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> OpenOnGitHub { get; }
    }
}
