using System;
using GitHub.ViewModels;

namespace GitHub.UI
{
    /// <summary>
    /// Base interface for all views.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Gets the view model associated with the view.
        /// </summary>
        IViewModel ViewModel { get; }

        /// <summary>
        /// Gets or sets the WPF DataContext for the view.
        /// </summary>
        object DataContext { get; set; }
    }
}
