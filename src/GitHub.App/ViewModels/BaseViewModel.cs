using System.Windows.Input;
using ReactiveUI;
using NullGuard;
using GitHub.UI;

namespace GitHub.ViewModels
{
    public class BaseViewModel : ReactiveObject, IReactiveViewModel
    {
        protected ObservableAsPropertyHelper<bool> isShowing;
        bool isBusy;

        public BaseViewModel()
        {
            CancelCommand = ReactiveCommand.Create();
        }

        public IReactiveCommand<object> CancelCommand { get; protected set; }
        public ICommand Cancel { get { return CancelCommand; } }

        public string Title {[return: AllowNull] get; protected set; }
        public bool IsShowing { get { return isShowing?.Value ?? true; } }
        public bool IsBusy
        {
            get { return isBusy; }
            set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        public virtual void Initialize([AllowNull] ViewWithData data)
        {
        }

        public virtual void Reset()
        {
        }
    }
}
