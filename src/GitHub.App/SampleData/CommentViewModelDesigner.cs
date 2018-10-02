using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    public class CommentViewModelDesigner : ReactiveObject, ICommentViewModel
    {
        public CommentViewModelDesigner()
        {
            Author = new ActorViewModel { Login = "shana" };
        }

        public string Id { get; set; }
        public int PullRequestId { get; set; }
        public int DatabaseId { get; set; }
        public string Body { get; set; }
        public string ErrorMessage { get; set; }
        public CommentEditState EditState { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSubmitting { get; set; }
        public bool CanDelete { get; } = true;
        public ICommentThreadViewModel Thread { get; }
        public DateTimeOffset UpdatedAt => DateTime.Now.Subtract(TimeSpan.FromDays(3));
        public IActorViewModel Author { get; set; }
        public Uri WebUrl { get; }

        public ReactiveCommand<object> BeginEdit { get; }
        public ReactiveCommand<object> CancelEdit { get; }
        public ReactiveCommand<Unit> CommitEdit { get; }
        public ReactiveCommand<object> OpenOnGitHub { get; }
        public ReactiveCommand<Unit> Delete { get; }
    }
}
