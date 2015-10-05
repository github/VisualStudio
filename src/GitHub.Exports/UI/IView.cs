using System;

namespace GitHub.UI
{
    public interface IView : IDisposable
    {
        object ViewModel { get; set; }
        IObservable<object> Done { get; }
        IObservable<object> Cancel { get; }
        IObservable<bool> IsBusy { get; }
    }
}
