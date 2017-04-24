using System;
using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    enum CommentState
    {
        None,
        Editing,
        Placeholder,
    }

    interface ICommentViewModel
    {
        string Body { get; set; }
        string ErrorMessage { get; }
        CommentState State { get; }
        DateTimeOffset UpdatedAt { get; }
        IAccount User { get; }

        ReactiveCommand<object> BeginEdit { get; }
        ReactiveCommand<object> CancelEdit { get; }
        ReactiveCommand<Unit> CommitEdit { get; }
    }
}