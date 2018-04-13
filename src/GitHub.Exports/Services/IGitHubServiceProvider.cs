using System;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GitHub.VisualStudio;

namespace GitHub.Services
{
    [Guid(Guids.GitHubServiceProviderId)]
    public interface IGitHubServiceProvider : IServiceProvider
    {
        ExportProvider ExportProvider { get; }
        IServiceProvider GitServiceProvider { get; set; }

        void AddService(Type t, object owner, object instance);
        void AddService<T>(object owner, T instance) where T : class;
        /// <summary>
        /// Removes a service from the catalog
        /// </summary>
        /// <param name="t">The type we want to remove</param>
        /// <param name="owner">The owner, which either has to match what was passed to AddService,
        /// or if it's null, the service will be removed without checking for ownership</param>
        void RemoveService(Type t, object owner);

        T TryGetServiceSync<T>() where T : class;

        Task<T> TryGetServiceAsync<T>() where T : class;
        Task<T1> TryGetServiceAsync<T, T1>() where T : class where T1 : class;

        Task<T> TryGetServiceMainThread<T>() where T : class;

        T TryGetMEFComponent<T>() where T : class;
        object TryGetMEFComponent(Type serviceType);

        T1 TryGetMEFComponent<T, T1>() where T : class where T1 : class;

        T GetMEFComponent<T>() where T : class;
        T1 GetMEFComponent<T, T1>() where T : class where T1 : class;
    }
}
