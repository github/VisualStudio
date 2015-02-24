using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IUIProvider
    {
        ExportProvider ExportProvider { get; }
        object GetService(Type t);
        T GetService<T>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        Ret GetService<T, Ret>() where Ret : class;
    }
}
