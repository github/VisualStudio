using System;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using GitHub.VisualStudio;

namespace GitHub.Services
{
    [Guid(Guids.GitHubServiceProviderId)]
    public interface IGitHubServiceProvider : IServiceProvider
    {
        ExportProvider ExportProvider { get; }
        IServiceProvider GitServiceProvider { get; set; }

        T GetService<T>() where T : class;
        Ret GetService<T, Ret>() where T : class
                                 where Ret : class;

        object TryGetService(Type t);
        object TryGetService(string typename);
        T TryGetService<T>() where T : class;

        void AddService(Type t, object owner, object instance);
        void AddService<T>(object owner, T instance) where T : class;
        /// <summary>
        /// Removes a service from the catalog
        /// </summary>
        /// <param name="t">The type we want to remove</param>
        /// <param name="owner">The owner, which either has to match what was passed to AddService,
        /// or if it's null, the service will be removed without checking for ownership</param>
        void RemoveService(Type t, object owner);

        /// <summary>
        /// Gets a Visual Studio service asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        Task<T> GetServiceAsync<T>() where T : class;

        /// <summary>
        /// Gets a Visual Studio service asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <typeparam name="Ret">The type of the service instance to return.</typeparam>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        Task<Ret> GetServiceAsync<T, Ret>() where T : class where Ret : class;
    }
}
