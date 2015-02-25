using Microsoft.VisualStudio.ComponentModelHost;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel.Composition.Primitives;
using Microsoft.VisualStudio.Shell;
using System.Globalization;
using GitHub.VisualStudio.TeamExplorerConnect;
using GitHub.Services;
using GitHub.VisualStudio;
using Moq;

namespace DesignTimeStyleHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static CustomServiceProvider ServiceProvider { get; private set; }

        static App()
        {
            ServiceProvider = new CustomServiceProvider();
        }
    }

    [Export(typeof(SVsServiceProvider))]
    [Export(typeof(SComponentModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CustomServiceProvider : SVsServiceProvider, IServiceProvider,
        SComponentModel, IComponentModel
    {
        CompositionContainer container;
        public CompositionContainer Container { get { return container; } }
        AggregateCatalog catalog;

        public CustomServiceProvider()
        {
            catalog = new AggregateCatalog(
                            new AssemblyCatalog(typeof(GitHub.VisualStudio.Services).Assembly), // GitHub.VisualStudio
                            new AssemblyCatalog(typeof(GitHub.Api.ApiClient).Assembly), // GitHub.App
                            new AssemblyCatalog(typeof(GitHub.Api.SimpleApiClient).Assembly), // GitHub.Api
                            new AssemblyCatalog(typeof(Rothko.Environment).Assembly), // GitHub.Api
                            new AssemblyCatalog(typeof(GitHub.Services.EnterpriseProbeTask).Assembly) // GitHub.Exports
                            );
            container = new CompositionContainer(catalog, CompositionOptions.IsThreadSafe | CompositionOptions.DisableSilentRejection);

            DefaultExportProvider = container;
            DefaultCompositionService = catalog.CreateCompositionService();

            var batch = new CompositionBatch();
            batch.AddExportedValue<SVsServiceProvider>(this);
            batch.AddExportedValue<SComponentModel>(this);
            batch.AddExportedValue(new PlaceholderGitHubSection(this));
            container.Compose(batch);
        }


        public object GetService(Type serviceType)
        {
            string contract = AttributedModelServices.GetContractName(serviceType);
            var instance = container.GetExportedValues<object>(contract).FirstOrDefault();

            if (instance != null)
                return instance;

            if (serviceType == typeof(IUIProvider))
                return new UIProvider(this);
            if (serviceType == typeof(ExportFactoryProvider))
                return new ExportFactoryProvider(DefaultCompositionService);

            instance = Create(serviceType);

            if (instance != null)
                return instance;

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Could not locate any instances of contract {0}.", contract));
        }

        T Create<T>() where T : class
        {
            return new Mock<T>().Object;
        }

        object Create(Type t)
        {
            var moq = typeof(Mock<>).MakeGenericType(t);
            var ctor = moq.GetConstructor(new Type[] { });
            var m = ctor.Invoke(new object[] { }) as Mock;
            return m.Object;
        }


        public ExportProvider DefaultExportProvider { get; set; }
        public ComposablePartCatalog DefaultCatalog { get; set; }
        public ICompositionService DefaultCompositionService { get; set; }

        public ComposablePartCatalog GetCatalog(string catalogName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetExtensions<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public T GetService<T>() where T : class
        {
            throw new NotImplementedException();
        }
    }
}
