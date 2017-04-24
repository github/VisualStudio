using System;
using System.Reactive;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.SampleData;
using ReactiveUI;

namespace GitHub.InlineReviews.SampleData
{
    class PullRequestReviewCommentDesigner : ICommentViewModel
    {
        public PullRequestReviewCommentDesigner()
        {
            User = new AccountDesigner { Login = "shana", IsUser = true };
        }

        public string Body { get; set; }
        public CommentState State { get; set; }
        public DateTimeOffset UpdatedAt => DateTime.Now.Subtract(TimeSpan.FromDays(3));
        public IAccount User { get; set; }

        public ReactiveCommand<object> BeginEdit { get; }
        public ReactiveCommand<object> CancelEdit { get; }
        public ReactiveCommand<Unit> CommitEdit { get; }
    }
}
