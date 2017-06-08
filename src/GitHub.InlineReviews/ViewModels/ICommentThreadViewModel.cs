using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// A comment thread.
    /// </summary>
    public interface ICommentThreadViewModel
    {
        /// <summary>
        /// Gets the comments in the thread.
        /// </summary>
        ObservableCollection<ICommentViewModel> Comments { get; }

        /// <summary>
        /// Gets the current user under whos account new comments will be created.
        /// </summary>
        IAccount CurrentUser { get; }

        /// <summary>
        /// Adds a reply placeholder to the end of the <see cref="Comments"/>.
        /// </summary>
        /// <returns></returns>
        ICommentViewModel AddReplyPlaceholder();

        /// <summary>
        /// Called by a comment in the thread to post itself as a new comment to the API.
        /// </summary>
        /// <param name="body">The comment body.</param>
        /// <returns>The comment model of the generated comment.</returns>
        Task<ICommentModel> PostComment(string body);
    }
}
