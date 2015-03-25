using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using GitHub.Services;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NullGuard;
using System.Reflection;

namespace GitHub.VisualStudio
{
    [Export(typeof(IUIProvider))]
    [Export(typeof(IServiceProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UIProvider : IServiceProvider, IUIProvider
    {
        [AllowNull]
        public ExportProvider ExportProvider { get; private set; }

        IServiceProvider gitServiceProvider;
        [AllowNull]
        public IServiceProvider GitServiceProvider {
            get { return gitServiceProvider; }
            set {
                if (gitServiceProvider == null)
                    gitServiceProvider = value;
            }
        }

        readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public UIProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            var componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Debug.Assert(componentModel != null, "Service of type SComponentModel not found");
            ExportProvider = componentModel.DefaultExportProvider;
        }

        [return: AllowNull]
        public object TryGetService(Type serviceType)
        {
            string contract = AttributedModelServices.GetContractName(serviceType);
            var instance = ExportProvider.GetExportedValues<object>(contract).FirstOrDefault();

            if (instance != null)
                return instance;

            instance = serviceProvider.GetService(serviceType);
            if (instance != null)
                return instance;

            if (gitServiceProvider != null)
            {
                instance = gitServiceProvider.GetService(serviceType);
                if (instance != null)
                    return instance;
            }

            return null;
        }

        public object GetService(Type serviceType)
        {
            var instance = TryGetService(serviceType);
            if (instance != null)
                return instance;

            string contract = AttributedModelServices.GetContractName(serviceType);
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Could not locate any instances of contract {0}.", contract));
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public T TryGetService<T>() where T : class
        {
            return TryGetService(typeof(T)) as T;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public Ret GetService<T, Ret>() where Ret : class
        {
            return GetService<T>() as Ret;
        }
    }
}
