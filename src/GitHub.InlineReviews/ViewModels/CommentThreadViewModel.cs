using System;
using System.Collections.ObjectModel;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Base view model for a thread of comments.
    /// </summary>
    public abstract class CommentThreadViewModel : ReactiveObject, ICommentThreadViewModel
    {
        ReactiveCommand<ICommentModel> postComment;
        private ReactiveCommand<ICommentModel> editComment;
        private ReactiveCommand<object> deleteComment;

        /// <summary>
        /// Intializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="currentUser">The current user.</param>
        protected CommentThreadViewModel(IAccount currentUser)
        {
            Guard.ArgumentNotNull(currentUser, nameof(currentUser));

            Comments = new ObservableCollection<ICommentViewModel>();
            CurrentUser = currentUser;
        }

        /// <inheritdoc/>
        public ObservableCollection<ICommentViewModel> Comments { get; }

        /// <inheritdoc/>
        public ReactiveCommand<ICommentModel> PostComment
        {
            get { return postComment; }
            protected set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                postComment = value;

                // We want to ignore thrown exceptions from PostComment - the error should be handled
                // by the CommentViewModel that trigged PostComment.Execute();
                value.ThrownExceptions.Subscribe(_ => { });
            }
        }

        public ReactiveCommand<ICommentModel> EditComment
        {
            get { return editComment; }
            protected set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                editComment = value;

                value.ThrownExceptions.Subscribe(_ => { });
            }
        }

        public ReactiveCommand<object> DeleteComment
        {
            get { return deleteComment; }
            protected set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                deleteComment = value;

                value.ThrownExceptions.Subscribe(_ => { });
            }
        }

        /// <inheritdoc/>
        public IAccount CurrentUser { get; }

        /// <inheritdoc/>
        public abstract Uri GetCommentUrl(int id);
    }
}
