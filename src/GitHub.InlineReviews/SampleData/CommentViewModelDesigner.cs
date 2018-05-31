using System;
using System.Reactive;
using System.Diagnostics.CodeAnalysis;
using GitHub.InlineReviews.ViewModels;
using ReactiveUI;
using GitHub.ViewModels;

namespace GitHub.InlineReviews.SampleData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class CommentViewModelDesigner : ReactiveObject, ICommentViewModel
    {
        public CommentViewModelDesigner()
        {
            Author = new ActorViewModel { Login = "shana" };
        }

        public string Id { get; set; }
        public string Body { get; set; }
        public string ErrorMessage { get; set; }
        public CommentEditState EditState { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSubmitting { get; set; }
        public ICommentThreadViewModel Thread { get; }
        public DateTimeOffset UpdatedAt => DateTime.Now.Subtract(TimeSpan.FromDays(3));
        public IActorViewModel Author { get; set; }

        public ReactiveCommand<object> BeginEdit { get; }
        public ReactiveCommand<object> CancelEdit { get; }
        public ReactiveCommand<Unit> CommitEdit { get; }
        public ReactiveCommand<object> OpenOnGitHub { get; }
    }
}
