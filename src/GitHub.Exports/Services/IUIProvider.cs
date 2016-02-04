using System;
using System.ComponentModel.Composition.Hosting;
using GitHub.Models;
using GitHub.UI;
using System.Windows.Controls;

namespace GitHub.Services
{
    public interface IUIProvider : IServiceProvider
    {
        ExportProvider ExportProvider { get; }
        IServiceProvider GitServiceProvider { get; set; }

        object TryGetService(Type t);

        void AddService(Type t, object instance);
        void AddService<T>(T instance);
        void RemoveService(Type t);

        IObservable<UserControl> SetupUI(UIControllerFlow controllerFlow, IConnection connection);
        void RunUI();
        void RunUI(UIControllerFlow controllerFlow, IConnection connection);
        IObservable<bool> ListenToCompletionState();
    }
}
