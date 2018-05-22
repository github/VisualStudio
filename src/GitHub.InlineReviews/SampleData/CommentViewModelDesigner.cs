using System;
using System.Reactive;
using System.Diagnostics.CodeAnalysis;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.SampleData;
using GitHub.UI;
using ReactiveUI;

namespace GitHub.InlineReviews.SampleData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class CommentViewModelDesigner : ReactiveObject, ICommentViewModel
    {
        public CommentViewModelDesigner()
        {
            User = new AccountDesigner { Login = "shana", IsUser = true };
        }

        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Body { get; set; }
        public string ErrorMessage { get; set; }
        public CommentEditState EditState { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSubmitting { get; set; }
        public ICommentThreadViewModel Thread { get; }
        public DateTimeOffset UpdatedAt => DateTime.Now.Subtract(TimeSpan.FromDays(3));
        public IAccount User { get; set; }

        public ReactiveCommand<object> BeginCreate { get; }
        public ReactiveCommand<object> CancelCreate { get; }
        public ReactiveCommand<Unit> CommitCreate { get; }
        public ReactiveCommand<object> BeginEdit { get; }
        public ReactiveCommand<object> CancelEdit { get; }
        public ReactiveCommand<Unit> CommitEdit { get; }
        public ReactiveCommand<object> OpenOnGitHub { get; }
        public ReactiveCommand<Unit> Delete { get; }
    }
}
