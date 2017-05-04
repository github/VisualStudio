using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Base view model for a thread of comments.
    /// </summary>
    abstract class CommentThreadViewModel : ICommentThreadViewModel, IDisposable
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
        public IAccount CurrentUser { get; }

        /// <inheritdoc/>
        public ICommentViewModel AddReplyPlaceholder()
        {
            var placeholder = CreateReplyPlaceholder();

            placeholderSubscription = placeholder.CommitEdit.Subscribe(_ =>
            {
                placeholderSubscription.Dispose();
                placeholderSubscription = null;
                AddReplyPlaceholder();
            });

            Comments.Add(placeholder);

            return placeholder;
        }

        /// <inheritdoc/>
        public abstract Task<ICommentModel> PostComment(string body);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract ICommentViewModel CreateReplyPlaceholder();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                placeholderSubscription?.Dispose();
            }
        }
    }
}
