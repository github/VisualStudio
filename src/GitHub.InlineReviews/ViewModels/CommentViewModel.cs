using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// View model for an issue or pull request comment.
    /// </summary>
    public class CommentViewModel : ReactiveObject, ICommentViewModel
    {
        string body;
        string errorMessage;
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

            CancelEdit = ReactiveCommand.Create();
            CancelEdit.Subscribe(DoCancelEdit);

            CommitEdit = ReactiveCommand.CreateAsyncTask(
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.Body, x => !string.IsNullOrEmpty(x)),
                    this.WhenAnyObservable(x => x.Thread.PostComment.CanExecuteObservable),
                    (hasBody, canPost) => hasBody && canPost),
                DoCommitEdit);
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
            : this(thread, currentUser, model.Id, model.Body, CommentEditState.None, model.User, model.UpdatedAt)
        {
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
                ErrorMessage = e.Message;
            }
        }
    }
}
