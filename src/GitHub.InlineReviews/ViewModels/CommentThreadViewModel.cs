using System;
using System.Collections.ObjectModel;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Base view model for a thread of comments.
    /// </summary>
    public abstract class CommentThreadViewModel : ReactiveObject, ICommentThreadViewModel
    {
        ReactiveCommand<CommentModel> postComment;

        /// <summary>
        /// Intializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="commentModels">The thread comments.</param>
        protected CommentThreadViewModel(ActorModel currentUser)
        {
            Guard.ArgumentNotNull(currentUser, nameof(currentUser));

            Comments = new ObservableCollection<ICommentViewModel>();
            CurrentUser = new ActorViewModel(currentUser);
        }

        /// <inheritdoc/>
        public ObservableCollection<ICommentViewModel> Comments { get; }

        /// <inheritdoc/>
        public ReactiveCommand<CommentModel> PostComment
        {
            get { return postComment; }
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                postComment = value;

                // We want to ignore thrown exceptions from PostComment - the error should be handled
                // by the CommentViewModel that trigged PostComment.Execute();
                value.ThrownExceptions.Subscribe(_ => { });
            }
        }

        /// <inheritdoc/>
        public IActorViewModel CurrentUser { get; }

        /// <inheritdoc/>
        public abstract Uri GetCommentUrl(int id);
    }
}
