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
        object TryGetService(string typename);
        T TryGetService<T>() where T : class;

        void AddService(Type t, object owner, object instance);
        void AddService<T>(object owner, T instance);
        /// <summary>
        /// Removes a service from the catalog
        /// </summary>
        /// <param name="t">The type we want to remove</param>
        /// <param name="owner">The owner, which either has to match what was passed to AddService,
        /// or if it's null, the service will be removed without checking for ownership</param>
        void RemoveService(Type t, object owner);

        IObservable<UserControl> SetupUI(UIControllerFlow controllerFlow, IConnection connection);
        void RunUI();
        void RunUI(UIControllerFlow controllerFlow, IConnection connection);
        IObservable<bool> ListenToCompletionState();
    }
}
