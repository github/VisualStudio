using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    class CommentViewModel : ReactiveObject, ICommentViewModel
    {
        string body;
        string undoBody;
        CommentState state;

        public CommentViewModel(
            CommentThreadViewModel thread,
            IAccount currentUser,
            IPullRequestReviewCommentModel model)
            : this(thread, currentUser, model.Id, model.Body, CommentState.None, model.User, model.UpdatedAt)
        {
        }

        public CommentViewModel(
            CommentThreadViewModel thread,
            IAccount currentUser,
            int commentId,
            string body,
            CommentState state,
            IAccount user,
            DateTimeOffset updatedAt)
        {
            Thread = thread;
            CurrentUser = currentUser;
            CommentId = commentId;
            Body = body;
            State = state;
            User = user;
            UpdatedAt = updatedAt;

            var canEdit = this.WhenAnyValue(
                x => x.State,
                x => x == CommentState.Placeholder || (x == CommentState.None && user.Equals(currentUser)));

            BeginEdit = ReactiveCommand.Create(canEdit);
            BeginEdit.Subscribe(DoBeginEdit);

            CancelEdit = ReactiveCommand.Create();
            CancelEdit.Subscribe(DoCancelEdit);

            CommitEdit = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Body, x => !string.IsNullOrEmpty(x)),
                DoCommitEdit);
        }

        public string Body
        {
            get { return body; }
            set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        public CommentState State
        {
            get { return state; }
            private set { this.RaiseAndSetIfChanged(ref state, value); }
        }

        public int CommentId { get; private set; }
        public IAccount CurrentUser { get; }
        public CommentThreadViewModel Thread { get; }
        public IAccount User { get; }
        public DateTimeOffset UpdatedAt { get; }

        public ReactiveCommand<object> BeginEdit { get; }
        public ReactiveCommand<object> CancelEdit { get; }
        public ReactiveCommand<Unit> CommitEdit { get; }

        public static CommentViewModel CreatePlaceholder(
            CommentThreadViewModel thread,
            IAccount currentUser)
        {
            return new CommentViewModel(
                thread,
                currentUser,
                0,
                null,
                CommentState.Placeholder,
                currentUser,
                DateTimeOffset.MinValue);
        }

        void DoBeginEdit(object unused)
        {
            if (state != CommentState.Editing)
            {
                undoBody = Body;
                State = CommentState.Editing;
            }
        }

        void DoCancelEdit(object unused)
        {
            if (State == CommentState.Editing)
            {
                State = undoBody == null ? CommentState.Placeholder : CommentState.None;
                Body = undoBody;
                undoBody = null;
            }
        }

        Task DoCommitEdit(object unused)
        {
            throw new NotImplementedException();
        }
    }
}
