using System;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    class IssueCommentThreadViewModel : CommentThreadViewModel
    {
        public IssueCommentThreadViewModel(
            IRepositoryModel repository,
            int number,
            IAccount currentUser)
            : base(currentUser)
        {
            Repository = repository;
            Number = number;
        }

        public IRepositoryModel Repository { get; }
        public int Number { get; }

        /// <inheritdoc/>
        public override ReactiveCommand<ICommentModel> PostComment { get; }
    }
}
