using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.SampleData;

namespace GitHub.InlineReviews.SampleData
{
    class CommentThreadViewModelDesigner : ICommentThreadViewModel
    {
        public ObservableCollection<ICommentViewModel> Comments { get; }
            = new ObservableCollection<ICommentViewModel>();
 
        public IAccount CurrentUser { get; set; }
            = new AccountDesigner { Login = "shana", IsUser = true };

        public ICommentViewModel AddReplyPlaceholder()
        {
            throw new NotImplementedException();
        }

        public Task<ICommentModel> PostComment(string body)
        {
            throw new NotImplementedException();
        }
    }
}
