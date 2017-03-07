using ReactiveUI;
using NullGuard;
using GitHub.UI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for view models that appear as a page in a navigable pane, such as the GitHub pane.
    /// </summary>
    public class PanePageViewModelBase : ReactiveObject, IPanePageViewModel
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
            [return: AllowNull]
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        /// <inheritdoc/>
        public virtual void Initialize([AllowNull] ViewWithData data)
        {
        }
    }
}
