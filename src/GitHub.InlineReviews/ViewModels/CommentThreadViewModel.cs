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
            IAccount currentUser,
            IEnumerable<InlineCommentModel> comments)
        {
            var commentViewModels = comments
                .Select(x => new CommentViewModel(this, currentUser, x.Original))
                .Concat(new[]
                {
                    CommentViewModel.CreatePlaceholder(this, currentUser),
                });

            Comments = new ObservableCollection<CommentViewModel>(commentViewModels);
        }

        public ObservableCollection<CommentViewModel> Comments { get; }
    }
}
