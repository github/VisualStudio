using System;
using System.Reactive;
using ReactiveUI;
using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    public class TooltipCommentViewModel : ICommentViewModel
    {
        internal TooltipCommentViewModel(IAccount user, string body, DateTimeOffset updatedAt)
        {
            User = user;
            Body = body;
            UpdatedAt = updatedAt;
        }

        public ReactiveCommand<object> BeginEdit { get; }

        public string Body { get; set; }

        public ReactiveCommand<object> CancelEdit { get; }

        public ReactiveCommand<Unit> CommitEdit { get; }

        public CommentEditState EditState { get; }

        public string ErrorMessage { get; }

        public int Id { get; }

        public DateTimeOffset UpdatedAt { get; }

        public IAccount User { get; }
    }

}
