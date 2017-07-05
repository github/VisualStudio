using System;
using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    public enum CommentEditState
    {
        None,
        Editing,
        Placeholder,
    }

    public interface ICommentViewModel
    {
        /// <summary>
        /// Gets the ID of the comment.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets the body of the comment.
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// Gets any error message encountered posting or updating the comment.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets the current edit state of the comment.
        /// </summary>
        CommentEditState EditState { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the comment is read-only.
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets the modified date of the comment.
        /// </summary>
        DateTimeOffset UpdatedAt { get; }

        /// <summary>
        /// Gets the author of the comment.
        /// </summary>
        IAccount User { get; }

        /// <summary>
        /// Gets a command which will begin editing of the comment.
        /// </summary>
        ReactiveCommand<object> BeginEdit { get; }

        /// <summary>
        /// Gets a command which will cancel editing of the comment.
        /// </summary>
        ReactiveCommand<object> CancelEdit { get; }

        /// <summary>
        /// Gets a command which will commit edits to the comment.
        /// </summary>
        ReactiveCommand<Unit> CommitEdit { get; }
    }
}