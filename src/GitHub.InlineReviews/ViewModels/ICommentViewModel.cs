using System;
using GitHub.Models;

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
        CommentState State { get; }
        DateTimeOffset UpdatedAt { get; }
        IAccount User { get; }

        void BeginEdit();
    }
}