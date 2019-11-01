using System;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for comments on an issue or pull request.
    /// </summary>
    public interface IIssueishCommentViewModel : ICommentViewModel, IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the comment will show a button for
        /// <see cref="CloseOrReopen"/>.
        /// </summary>
        bool CanCloseOrReopen { get; }

        /// <summary>
        /// Gets a a caption for the <see cref="CloseOrReopen"/> command.
        /// </summary>
        string CloseOrReopenCaption { get; }

        /// <summary>
        /// Gets a command which when executed will close the issue or pull request if it is open,
        /// or reopen it if it is closed.
        /// </summary>
        ReactiveCommand<Unit, Unit> CloseOrReopen { get; }

        /// <summary>
        /// Initializes the view model with data.
        /// </summary>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="comment">The comment model. May be null.</param>
        /// <param name="isPullRequest">
        /// true if the comment is on a pull request, false if the comment is on an issue.
        /// </param>
        /// <param name="canCloseOrReopen">
        /// Whether the user can close or reopen the pull request from this comment.
        /// </param>
        /// <param name="isOpen">
        /// An observable tracking whether the issue or pull request is open. Can be null if
        /// <paramref name="canCloseOrReopen"/> is false.
        /// </param>
        Task InitializeAsync(
            IIssueishCommentThreadViewModel thread,
            ActorModel currentUser,
            CommentModel comment,
            bool isPullRequest,
            bool canCloseOrReopen,
            IObservable<bool> isOpen = null);
    }
}