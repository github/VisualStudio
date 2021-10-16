using System;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    /// <summary>
    /// Base class for view models representing a multi-page dialog.
    /// </summary>
    public abstract class PagedDialogViewModelBase : ViewModelBase, IDialogContentViewModel
    {
        readonly ObservableAsPropertyHelper<string> title;
        IDialogContentViewModel content;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedDialogViewModelBase"/> class.
        /// </summary>
        protected PagedDialogViewModelBase()
        {
            title = this.WhenAny(x => x.Content, x => x.Value?.Title ?? "GitHub")
                .ToProperty(this, x => x.Title);
        }

        /// <summary>
        /// Gets the current page being displayed in the dialog.
        /// </summary>
        public IDialogContentViewModel Content
        {
            get { return content; }
            protected set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        /// <inheritdoc/>
        public abstract IObservable<object> Done { get; }

        /// <inheritdoc/>
        public string Title => title.Value;
    }
}
