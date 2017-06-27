using System;
using System.Collections.ObjectModel;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.SampleData;
using ReactiveUI;

namespace GitHub.InlineReviews.SampleData
{
    class CommentTooltipViewModelDesigner : ICommentTooltipViewModel
    {
        public ObservableCollection<ICommentViewModel> Comments { get; }
            = new ObservableCollection<ICommentViewModel>();
    }
}
