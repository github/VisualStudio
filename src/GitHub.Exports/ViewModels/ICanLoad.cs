using System;
using GitHub.UI;

namespace GitHub.ViewModels
{
    public interface ICanLoad
    {
        IObservable<ViewWithData> Load { get; }
    }
}
