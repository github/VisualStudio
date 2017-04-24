using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GitHub.InlineReviews.Models;
using GitHub.Models;

namespace GitHub.InlineReviews.ViewModels
{
    class CommentThreadViewModel
    {
        public CommentThreadViewModel(
            IAccount user,
            IEnumerable<InlineCommentModel> comments)
        {
            var commentViewModels = comments
                .Select(x => new CommentViewModel(this, x.Original))
                .Concat(new[]
                {
                    new CommentViewModel(this, user),
                });

            Comments = new ObservableCollection<CommentViewModel>(commentViewModels);
        }

        public ObservableCollection<CommentViewModel> Comments { get; }
    }
}
