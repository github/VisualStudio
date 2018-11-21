using System;
using System.Threading.Tasks;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A comment thread.
    /// </summary>
    public interface ICommentThreadViewModel : IViewModel
    {
        /// <summary>
        /// Gets the comments in the thread.
        /// </summary>
        IReadOnlyReactiveList<ICommentViewModel> Comments { get; }

        /// <summary>
        /// Gets the current user under whos account new comments will be created.
        /// </summary>
        IActorViewModel CurrentUser { get; }

        /// <summary>
        /// Called by a comment in the thread to post itself as a new comment to the API.
        /// </summary>
        /// <param name="comment">The comment to post.</param>
        Task PostComment(ICommentViewModel comment);

        /// <summary>
        /// Called by a comment in the thread to post itself as an edit to a comment to the API.
        /// </summary>
        /// <param name="comment">The comment to edit.</param>
        Task EditComment(ICommentViewModel comment);

        /// <summary>
        /// Called by a comment in the thread to delete the comment on the API.
        /// </summary>
        /// <param name="comment">The comment to delete.</param>
        Task DeleteComment(ICommentViewModel comment);
    }
}
