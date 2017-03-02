using System;

namespace GitHub.UI
{
    public interface IDialogView : IView
    {
        IObservable<object> Done { get; }
        IObservable<object> Cancel { get; }
        IObservable<bool> IsBusy { get; }
    }

    public interface ICanLoad
    {
        IObservable<ViewWithData> Load { get; }
    }
}
