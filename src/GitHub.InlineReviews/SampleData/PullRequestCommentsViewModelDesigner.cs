using System;
using System.Diagnostics.CodeAnalysis;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.SampleData
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
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
