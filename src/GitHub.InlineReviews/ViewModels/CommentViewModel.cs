using System;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    class CommentViewModel : ReactiveObject, ICommentViewModel
    {
        string body;
        CommentState state;

        public CommentViewModel(
            CommentThreadViewModel thread,
            IPullRequestReviewCommentModel model)
            : this(thread, model.Body, model.User, model.UpdatedAt)
        {
        }

        public CommentViewModel(
            CommentThreadViewModel thread,
            string body,
            IAccount user,
            DateTimeOffset updatedAt)
        {
            Body = body;
            State = CommentState.None;
            Thread = thread;
            User = user;
            UpdatedAt = updatedAt;
        }

        public CommentViewModel(
            CommentThreadViewModel thread,
            IAccount user)
        {
            Thread = thread;
            State = CommentState.Placeholder;
            User = user;
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

        public CommentThreadViewModel Thread { get; }
        public IAccount User { get; }
        public DateTimeOffset UpdatedAt { get; }

        public void BeginEdit()
        {
            State = CommentState.Editing;
        }
    }
}
