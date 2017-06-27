using System;
using System.Collections.ObjectModel;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// Base view model for a tooltip that displays comments.
    /// </summary>
    public class CommentTooltipViewModel : ICommentTooltipViewModel
    {
        /// <inheritdoc/>
        public ObservableCollection<ICommentViewModel> Comments { get; } =
            new ObservableCollection<ICommentViewModel>();
    }
}
