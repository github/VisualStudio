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
        IDisposable placeholderSubscription;

        /// <summary>
        /// Intializes a new instance of the <see cref="CommentThreadViewModel"/> class.
        /// </summary>
        /// <param name="currentUser">The current user.</param>
        /// <param name="commentModels">The thread comments.</param>
        public CommentThreadViewModel(IAccount currentUser)
        {
            Guard.ArgumentNotNull(currentUser, nameof(currentUser));

            Comments = new ObservableCollection<ICommentViewModel>();
            CurrentUser = currentUser;
        }

        /// <inheritdoc/>
        public ObservableCollection<ICommentViewModel> Comments { get; }

        /// <inheritdoc/>
        public abstract ReactiveCommand<ICommentModel> PostComment { get; }

        /// <inheritdoc/>
        public IAccount CurrentUser { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                placeholderSubscription?.Dispose();
            }
        }
    }
}
