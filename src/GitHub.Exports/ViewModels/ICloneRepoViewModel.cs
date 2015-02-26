using System;
using System.Windows.Input;

namespace GitHub.ViewModels
{
    public interface ICloneRepoViewModel : IViewModel
    {
        ICommand OkCmd { get; }
        ICommand CancelCmd { get; }
        IObservable<object> Cancelling { get; }
    }
}
