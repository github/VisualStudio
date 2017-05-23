using System;
using System.Collections.ObjectModel;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.SampleData
{
    class PullRequestCommentsViewModelDesigner : IPullRequestCommentsViewModel
    {
        public IRepositoryModel Repository { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public ICommentThreadViewModel Conversation { get; set; }
        public IReactiveList<IDiffCommentThreadViewModel> FileComments { get; } 
            = new ReactiveList<IDiffCommentThreadViewModel>();
    }
}
