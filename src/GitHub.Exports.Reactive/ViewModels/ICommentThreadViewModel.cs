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
        /// Gets the current user under whose account new comments will be created.
        /// </summary>
        IActorViewModel CurrentUser { get; }

        /// <summary>
        /// Called by a comment in the thread to post itself as a new comment to the API.
        /// </summary>
        Task PostComment(string body);

        /// <summary>
        /// Called by a comment in the thread to post itself as an edit to a comment to the API.
        /// </summary>
        Task EditComment(string id, string body);

        /// <summary>
        /// Called by a comment in the thread to delete the comment on the API.
        /// </summary>
        Task DeleteComment(int pullRequestId, int commentId);
    }
}
