using System.Windows.Input;
using ReactiveUI;
using NullGuard;
using GitHub.UI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for view models that can be dismissed, such as dialogs.
    /// </summary>
    public class DialogViewModelBase : ReactiveObject, IReactiveDialogViewModel, IHasBusy
    {
        protected ObservableAsPropertyHelper<bool> isShowing;
        string title;
        bool isBusy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogViewModelBase"/> class.
        /// </summary>
        protected DialogViewModelBase()
        {
            CancelCommand = ReactiveCommand.Create();
        }

        /// <inheritdoc/>
        public IReactiveCommand<object> CancelCommand { get; protected set; }

        /// <inheritdoc/>
        public ICommand Cancel { get { return CancelCommand; } }

        /// <inheritdoc/>
        public string Title
        {
            [return: AllowNull]
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        /// <inheritdoc/>
        public bool IsShowing { get { return isShowing?.Value ?? true; } }

        /// <inheritdoc/>
        public bool IsBusy
        {
            get { return isBusy; }
            set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        /// <inheritdoc/>
        public virtual void Initialize([AllowNull] ViewWithData data)
        {
        }
    }
}
