using System;
using System.Collections.ObjectModel;
using GitHub.InlineReviews.Models;
using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    class CommentBlockViewModel
    {
        public CommentBlockViewModel()
        {
            Comments = new ObservableCollection<IPullRequestReviewCommentModel>();
        }

        public ObservableCollection<IPullRequestReviewCommentModel> Comments { get; }
    }
}
