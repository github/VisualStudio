using System;
using System.Collections.ObjectModel;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// A comment thread.
    /// </summary>
    public interface ICommentThreadViewModel
    {
        /// <summary>
        /// Gets the browser URI for a comment in the thread.
        /// </summary>
        /// <param name="id">The ID of the comment.</param>
        /// <returns>The URI.</returns>
        Uri GetCommentUrl(int id);

        /// <summary>
        /// Gets the comments in the thread.
        /// </summary>
        ObservableCollection<ICommentViewModel> Comments { get; }

        /// <summary>
        /// Gets the current user under whos account new comments will be created.
        /// </summary>
        IActorViewModel CurrentUser { get; }

        /// <summary>
        /// Called by a comment in the thread to post itself as a new comment to the API.
        /// </summary>
        ReactiveCommand<CommentModel> PostComment { get; }

        /// <summary>
        /// Called by a comment in the thread to post itself as an edit to a comment to the API.
        /// </summary>
        ReactiveCommand<CommentModel> EditComment { get; }

        /// <summary>
        /// Called by a comment in the thread to send a delete of the comment to the API.
        /// </summary>
        ReactiveCommand<object> DeleteComment { get; }
    }
}
