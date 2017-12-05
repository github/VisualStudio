using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for view models that appear as a page in a navigable pane, such as the GitHub pane.
    /// </summary>
    public class PanePageViewModelBase : ViewModelBase, IPanePageViewModel
    {
        string title;

        /// <summary>
        /// Initializes a new instance of the <see cref="PanePageViewModelBase"/> class.
        /// </summary>
        protected PanePageViewModelBase()
        {
        }

        /// <inheritdoc/>
        public string Title
        {
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }
    }
}
