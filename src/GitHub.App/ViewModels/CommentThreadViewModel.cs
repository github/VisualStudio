using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base view model for a thread of comments.
    /// </summary>
    public abstract class CommentThreadViewModel : ReactiveObject, ICommentThreadViewModel
    {
        readonly ReactiveList<ICommentViewModel> comments = new ReactiveList<ICommentViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public CommentThreadViewModel()
        {
        }

        /// <summary>
        /// Intializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="currentUser">The current user.</param>
        protected Task InitializeAsync(ActorModel currentUser)
        {
            Guard.ArgumentNotNull(currentUser, nameof(currentUser));
            CurrentUser = new ActorViewModel(currentUser);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public IReactiveList<ICommentViewModel> Comments => comments;

        /// <inheritdoc/>
        public IActorViewModel CurrentUser { get; private set; }

        /// <inheritdoc/>
        IReadOnlyReactiveList<ICommentViewModel> ICommentThreadViewModel.Comments => comments;

        /// <inheritdoc/>
        public abstract Task PostComment(string body);

        /// <inheritdoc/>
        public abstract Task EditComment(string id, string body);

        /// <inheritdoc/>
        public abstract Task DeleteComment(int pullRequestId, int commentId);
    }
}
