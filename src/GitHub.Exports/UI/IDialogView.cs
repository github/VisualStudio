using System;

namespace GitHub.UI
{
    public interface IDialogView : IView, IHasDone, IHasCancel
    {
        IObservable<bool> IsBusy { get; }
    }

    public interface ICanLoad
    {
        IObservable<ViewWithData> Load { get; }
    }
}
