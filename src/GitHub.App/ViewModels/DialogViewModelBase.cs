using System;
using System.Reactive;
using GitHub.Extensions.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for view models that can be dismissed, such as dialogs.
    /// </summary>
    public abstract class DialogViewModelBase : ViewModelBase, IDialogViewModel, IHasBusy
    {
        protected ObservableAsPropertyHelper<bool> isShowing;
        string title;
        bool isBusy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogViewModelBase"/> class.
        /// </summary>
        protected DialogViewModelBase()
        {
            Cancel = ReactiveCommand.Create();
        }

        /// <inheritdoc/>
        public abstract IObservable<Unit> Done { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> Cancel { get; }

        /// <inheritdoc/>
        public string Title
        {
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        /// <inheritdoc/>
        public bool IsShowing { get { return isShowing?.Value ?? true; } }

        /// <inheritdoc/>
        public bool IsBusy
        {
            get { return isBusy; }
            protected set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        /// <inheritdoc/>
        IObservable<Unit> IHasCancel.Cancel => Cancel.SelectUnit();
    }
}
