using GitHub.Exports;
using System;

namespace GitHub.App.Factories
{
    public interface IUIFactory : IDisposable
    {
        IUIPair CreateViewAndViewModel(UIViewType viewType);
    }
}