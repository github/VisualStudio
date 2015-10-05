using System;
using System.ComponentModel.Composition.Hosting;
using GitHub.Models;
using GitHub.UI;
using System.Windows.Controls;

namespace GitHub.Services
{
    public interface IUIProvider
    {
        ExportProvider ExportProvider { get; }
        IServiceProvider GitServiceProvider { get; set; }
        object GetService(Type t);
        T GetService<T>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        Ret GetService<T, Ret>() where Ret : class;

        object TryGetService(Type t);
        T TryGetService<T>() where T : class;

        void AddService(Type t, object instance);
        void RemoveService(Type t);

        IObservable<IView> SetupUI(UIControllerFlow controllerFlow, IConnection connection);
        void RunUI();
        void RunUI(UIControllerFlow controllerFlow, IConnection connection);
    }
}
