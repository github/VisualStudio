using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for comments on an issue or pull request.
    /// </summary>
    public interface IIssueishCommentViewModel : ICommentViewModel
    {
        /// <summary>
        /// Gets a value indicating whether the comment will show a 
        /// a button for <see cref="CloseIssueish"/>.
        /// </summary>
        bool CanCloseIssueish { get; }

        /// <summary>
        /// Gets a a caption for the <see cref="CloseIssueish"/> command.
        /// </summary>
        string CloseIssueishCaption { get; }

        /// <summary>
        /// Gets a command which when executed will close the issue or pull request.
        /// </summary>
        ReactiveCommand<Unit, Unit> CloseIssueish { get; }

        /// <summary>
        /// Initializes the view model with data.
        /// </summary>
        /// <param name="thread">The thread that the comment is a part of.</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="comment">The comment model. May be null.</param>
        /// <param name="closeCaption">
        /// The caption for the <see cref="CloseIssueish"/> command, or null if the user cannot
        /// close the issue/pr from this comment.
        /// </param>
        Task InitializeAsync(
            ICommentThreadViewModel thread,
            ActorModel currentUser,
            CommentModel comment,
            string closeCaption);
    }
}