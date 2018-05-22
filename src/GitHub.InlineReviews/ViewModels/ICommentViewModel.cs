using System;
using System.Reactive;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    public enum CommentEditState
    {
        None,
        Creating,
        Editing,
        Placeholder,
    }

	/// <summary>
    /// View model for an issue or pull request comment.
    /// </summary>
    public interface ICommentViewModel : IViewModel
    {
        /// <summary>
        /// Gets the ID of the comment.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the GraphQL ID of the comment.
        /// </summary>
        string NodeId { get; }

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
        /// Gets a value indicating whether the comment is currently in the process of being
        /// submitted.
        /// </summary>
        bool IsSubmitting { get; }

        /// <summary>
        /// Gets the modified date of the comment.
        /// </summary>
        DateTimeOffset UpdatedAt { get; }

        /// <summary>
        /// Gets the author of the comment.
        /// </summary>
        IAccount User { get; }

        /// <summary>
        /// Gets the thread that the comment is a part of.
        /// </summary>
        ICommentThreadViewModel Thread { get; }

        /// <summary>
        /// Gets a command which will begin the creation of the comment.
        /// </summary>
        ReactiveCommand<object> BeginCreate { get; }

        /// <summary>
        /// Gets a command which will cancel the creation of the comment.
        /// </summary>
        ReactiveCommand<object> CancelCreate { get; }

        /// <summary>
        /// Gets a command which will commit the creation of the comment.
        /// </summary>
        ReactiveCommand<Unit> CommitCreate { get; }

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

        /// <summary>
        /// Gets a command to open the comment in a browser.
        /// </summary>
        ReactiveCommand<object> OpenOnGitHub { get; }

        /// <summary>
        /// Deletes a comment.
        /// </summary>
        ReactiveCommand<Unit> Delete { get; }
    }
}