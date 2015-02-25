using System;
using System.Windows.Input;

namespace GitHub.UI
{
    public interface ICloneRepoViewModel : IViewModel
    {
        ICommand OkCmd { get; }
        ICommand CancelCmd { get; }
        IObservable<object> Cancelling { get; }
    }
}
