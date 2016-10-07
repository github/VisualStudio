using GitHub.ViewModels;
using System;

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

    public interface IHasDetailView
    {
        IObservable<ViewWithData> Open { get; }
    }

    public interface IHasCreationView
    {
        IObservable<ViewWithData> Create { get; }
    }
}
