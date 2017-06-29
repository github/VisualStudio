using System;
using System.Collections.ObjectModel;
using GitHub.ViewModels;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Base view model for a tooltip that displays comments.
    /// </summary>
    public class CommentTooltipViewModel : ViewModelBase, ICommentTooltipViewModel
    {
        /// <inheritdoc/>
        public ObservableCollection<ICommentViewModel> Comments { get; } =
            new ObservableCollection<ICommentViewModel>();
    }
}
