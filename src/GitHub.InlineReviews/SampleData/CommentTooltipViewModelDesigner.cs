using System;
using System.Collections.ObjectModel;
using GitHub.ViewModels;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.SampleData
{
    class CommentTooltipViewModelDesigner : ViewModelBase, ICommentTooltipViewModel
    {
        public ObservableCollection<ICommentViewModel> Comments { get; }
            = new ObservableCollection<ICommentViewModel>();
    }
}
