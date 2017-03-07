using System;

namespace GitHub.UI
{
    public interface IHasCancel
    {
        IObservable<object> Cancel { get; }
    }
}
