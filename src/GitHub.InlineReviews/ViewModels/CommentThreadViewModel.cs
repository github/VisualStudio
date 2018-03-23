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
    public abstract class CommentThreadViewModel : ReactiveObject, ICommentThreadViewModel, IDisposable
    {
        ReactiveCommand<ICommentModel> postComment;
        IDisposable placeholderSubscription;

        /// <summary>
        /// Intializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="currentUser">The current user on null if not required.</param>
        /// <param name="commentModels">The thread comments.</param>
        protected CommentThreadViewModel(IAccount currentUser = null)
        {
            Comments = new ObservableCollection<ICommentViewModel>();
            CurrentUser = currentUser;
        }

        /// <inheritdoc/>
        public ObservableCollection<ICommentViewModel> Comments { get; }

        /// <inheritdoc/>
        public ReactiveCommand<ICommentModel> PostComment
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
        public IAccount CurrentUser { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public abstract Uri GetCommentUrl(int id);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                placeholderSubscription?.Dispose();
            }
        }
    }
}
