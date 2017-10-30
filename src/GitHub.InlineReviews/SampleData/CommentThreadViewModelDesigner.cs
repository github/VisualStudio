using System;
using System.Collections.ObjectModel;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.SampleData;
using ReactiveUI;

namespace GitHub.InlineReviews.SampleData
{
    public class CommentThreadViewModelDesigner : ICommentThreadViewModel
    {
        public ObservableCollection<ICommentViewModel> Comments { get; }
            = new ObservableCollection<ICommentViewModel>();

        public IAccount CurrentUser { get; set; }
            = new AccountDesigner { Login = "shana", IsUser = true };

        public ReactiveCommand<ICommentModel> PostComment { get; }

        public Uri GetCommentUrl(int id)
        {
            throw new NotImplementedException();
        }
    }
}
