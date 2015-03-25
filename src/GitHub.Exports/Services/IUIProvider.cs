using System;
using System.ComponentModel.Composition.Hosting;

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
    }
}
