using System.Windows.Input;
using ReactiveUI;
using NullGuard;
using GitHub.UI;

namespace GitHub.ViewModels
{
    public class BaseViewModel : ReactiveObject, IReactiveViewModel
    {
        protected ObservableAsPropertyHelper<bool> isShowing;

        public BaseViewModel()
        {
            CancelCommand = ReactiveCommand.Create();
        }

        public IReactiveCommand<object> CancelCommand { get; protected set; }
        public ICommand Cancel { get { return CancelCommand; } }

        public string Title {[return: AllowNull] get; protected set; }
        public bool IsShowing { get { return isShowing?.Value ?? true; } }

        public virtual void Initialize([AllowNull] ViewWithData data)
        {
        }
    }
}
