using System;
using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public enum CommentEditState
    {
        None,
        Editing,
        Placeholder,
    }

	/// <summary>
    /// View model for an issue or pull request comment.
    /// </summary>
    public interface ICommentViewModel : IViewModel
    {
        /// <summary>
        /// Gets the GraphQL ID of the comment.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the Database ID of the comment.
        /// </summary>
        int DatabaseId { get; }

        /// <summary>
        /// The pull request id of the comment
        /// </summary>
        int PullRequestId { get; }

        /// <summary>
        /// Gets the author of the comment.
        /// </summary>
        IActorViewModel Author { get; }

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
        /// Gets a value indicating whether the comment can be edited or deleted by the current user
        /// </summary>
        bool CanDelete { get; }

        /// <summary>
        /// Gets the creation date of the comment.
        /// </summary>
        DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets the thread that the comment is a part of.
        /// </summary>
        ICommentThreadViewModel Thread { get; }

        /// <summary>
        /// Gets the URL of the comment on the web.
        /// </summary>
        Uri WebUrl { get; }

        /// <summary>
        /// Gets a command which will begin editing of the comment.
        /// </summary>
        ReactiveCommand<Unit, Unit> BeginEdit { get; }

        /// <summary>
        /// Gets a command which will cancel editing of the comment.
        /// </summary>
        ReactiveCommand<Unit, Unit> CancelEdit { get; }

        /// <summary>
        /// Gets a command which will commit edits to the comment.
        /// </summary>
        ReactiveCommand<Unit, Unit> CommitEdit { get; }

        /// <summary>
        /// Gets a command to open the comment in a browser.
        /// </summary>
        ReactiveCommand<Unit, Unit> OpenOnGitHub { get; }

        /// <summary>
        /// Deletes a comment.
        /// </summary>
        ReactiveCommand<Unit, Unit> Delete { get; }
    }
}