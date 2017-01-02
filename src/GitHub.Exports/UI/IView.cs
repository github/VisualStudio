using GitHub.ViewModels;
using System;
using System.Windows.Input;

namespace GitHub.UI
{
    public interface IView
    {
        IViewModel ViewModel { get; }
        IObservable<object> Done { get; }
        IObservable<object> Cancel { get; }
        IObservable<bool> IsBusy { get; }
        // necessary for WPF to trigger binding events
        object DataContext { get; set; }

    }

    public interface IHasDetail
    {
        IObservable<int> Open { get; }
    }

    public interface IHasCreation
    {
        IObservable<object> Create { get; }
    }
}
