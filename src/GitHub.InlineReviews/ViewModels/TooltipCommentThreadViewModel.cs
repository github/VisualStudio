using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    public class TooltipCommentThreadViewModel : CommentThreadViewModel
    {
        public TooltipCommentThreadViewModel(IEnumerable<IPullRequestReviewCommentModel> comments)
        {
            foreach (var comment in comments)
            {
                Comments.Add(new CommentViewModel(this, CurrentUser, comment));
            }
        }

        public override Uri GetCommentUrl(int id)
        {
            throw new NotImplementedException();
        }
    }
}
