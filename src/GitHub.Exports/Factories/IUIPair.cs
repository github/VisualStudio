using GitHub.UI;
using GitHub.ViewModels;
using System;

namespace GitHub.App.Factories
{
    public interface IUIPair : IDisposable
    {
        IView View { get; }
        IViewModel ViewModel { get; }
        void AddHandler(IDisposable disposable);
        void ClearHandlers();
    }
}