using System;
using System.Threading.Tasks;
using GitHub.Models;

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

        public override Task<ICommentModel> PostComment(string body)
        {
            throw new NotImplementedException();
        }

        protected override ICommentViewModel CreateReplyPlaceholder()
        {
            return CommentViewModel.CreatePlaceholder(this, CurrentUser);
        }
    }
}
