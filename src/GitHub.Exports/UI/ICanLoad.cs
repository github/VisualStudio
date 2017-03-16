using System;

namespace GitHub.UI
{
    public interface ICanLoad
    {
        IObservable<ViewWithData> Load { get; }
    }
}
