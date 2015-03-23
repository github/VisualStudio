using System;

namespace GitHub.UI
{
    public interface IView
    {
        object ViewModel { get; set; }
        IObservable<object> Done { get; }
    }
}
