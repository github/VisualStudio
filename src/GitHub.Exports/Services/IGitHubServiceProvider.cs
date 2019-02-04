using System;
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
        TRet GetService<T, TRet>() where T : class
                                 where TRet : class;

        object TryGetService(Type t);
        object TryGetService(string typeName);
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
    }
}
