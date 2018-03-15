using System;
using System.Reactive;
using System.Threading.Tasks;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model that represents a page in the GitHub pane.
    /// </summary>
    public interface IPanePageViewModel : IViewModel, IDisposable
    {
        /// <summary>
        /// Gets an exception representing an error state to display.
        /// </summary>
        Exception Error { get; }

        /// <summary>
        /// Gets a value indicating whether the page is busy.
        /// </summary>
        /// <remarks>
        /// When <see cref="IsBusy"/> is set to true, an indeterminate progress bar will be
        /// displayed at the top of the GitHub pane but the pane contents will remain visible.
        /// </remarks>
        bool IsBusy { get; }

        /// <summary>
        /// Gets a value indicating whether the page is loading.
        /// </summary>
        /// <remarks>
        /// When <see cref="IsLoading"/> is set to true, a spinner will be displayed instead of the
        /// pane contents.
        /// </remarks>
        bool IsLoading { get; }

        /// <summary>
        /// Gets the title to display in the pane when the page is shown.
        /// </summary>
        string PaneTitle { get; }

        /// <summary>
        /// Gets an observable that is fired when the pane wishes to close itself.
        /// </summary>
        IObservable<Unit> CloseRequested { get; }

        /// <summary>
        /// Gets an observable that is fired with a URI when the pane wishes to navigate to another
        /// pane.
        /// </summary>
        IObservable<Uri> NavigationRequested { get; }

        /// <summary>
        /// Called when the page becomes the current page in the GitHub pane.
        /// </summary>
        void Activated();

        /// <summary>
        /// Called when the page stops being the current page in the GitHub pane.
        /// </summary>
        void Deactivated();

        /// <summary>
        /// Refreshes the view model.
        /// </summary>
        Task Refresh();
    }
}