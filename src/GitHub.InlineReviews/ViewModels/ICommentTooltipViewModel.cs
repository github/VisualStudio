using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using GitHub.Models;
using GitHub.ViewModels;

namespace GitHub.InlineReviews.ViewModels
{
    /// <summary>
    /// A comment tooltip.
    /// </summary>
    public interface ICommentTooltipViewModel : IViewModel
    {
        /// <summary>
        /// Gets the comments in the thread.
        /// </summary>
        ObservableCollection<ICommentViewModel> Comments { get; }
    }
}
