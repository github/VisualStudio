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

        public DialogViewModelBase()
        {
            CancelCommand = ReactiveCommand.Create();
        }

        public IReactiveCommand<object> CancelCommand { get; protected set; }
        public ICommand Cancel { get { return CancelCommand; } }

        public string Title
        {
            [return: AllowNull]
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        public bool IsShowing { get { return isShowing?.Value ?? true; } }
        public bool IsBusy
        {
            get { return isBusy; }
            set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        public virtual void Initialize([AllowNull] ViewWithData data)
        {
        }
    }
}
