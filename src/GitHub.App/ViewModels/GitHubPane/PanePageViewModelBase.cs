using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Base class for view models that appear as a page in a the GitHub pane.
    /// </summary>
    public abstract class PanePageViewModelBase : ViewModelBase, IPanePageViewModel, IDisposable
    {
        static readonly Uri paneUri = new Uri("github://pane");
        Subject<Uri> navigate = new Subject<Uri>();
        bool isBusy;
        bool isLoading;
        string title;

        /// <summary>
        /// Initializes a new instance of the <see cref="PanePageViewModelBase"/> class.
        /// </summary>
        protected PanePageViewModelBase()
        {
        }

        ~PanePageViewModelBase()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public bool IsBusy
        {
            get { return isBusy; }
            protected set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        /// <inheritdoc/>
        public bool IsLoading
        {
            get { return isLoading; }
            protected set { this.RaiseAndSetIfChanged(ref isLoading, value); }
        }

        /// <inheritdoc/>
        public string Title
        {
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        /// <inheritdoc/>
        public virtual IObservable<Unit> CloseRequested { get; }

        /// <inheritdoc/>
        public IObservable<Uri> NavigationRequested => navigate;

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public virtual Task Refresh() => Task.CompletedTask;

        /// <summary>
        /// Sends a request to navigate to a new page.
        /// </summary>
        /// <param name="uri">
        /// The path portion of the URI of the new page, e.g. "pulls".
        /// </param>
        protected void NavigateTo(string uri) => navigate.OnNext(new Uri(paneUri, new Uri(uri, UriKind.Relative)));

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                navigate?.Dispose();
                navigate = null;
            }
        }
    }
}
