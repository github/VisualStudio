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

        /// <inheritdoc/>
        public override Uri GetCommentUrl(int id)
        {
            throw new NotImplementedException();
        }

        public IRepositoryModel Repository { get; }
        public int Number { get; }
    }
}
