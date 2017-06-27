using System;
using System.Collections.ObjectModel;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// A comment tooltip.
    /// </summary>
    public interface ICommentTooltipViewModel
    {
        /// <summary>
        /// Gets the comments in the thread.
        /// </summary>
        ObservableCollection<ICommentViewModel> Comments { get; }
    }
}
