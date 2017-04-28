using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Windows;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Moq;
using GitHub.Models;

namespace DesignTimeStyleHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Holder ExportHolder { get; set; }
        public static CustomServiceProvider ServiceProvider { get { return (CustomServiceProvider) ExportHolder.ServiceProvider; } }
        

        public App()
        {
            var s = new CustomServiceProvider();
            ExportHolder = new Holder(s.DefaultCompositionService);
        }
    }

    public class Holder
    {
        [Import]
        public SVsServiceProvider ServiceProvider;

        [Import]
        public SComponentModel sc;

        [Import]
        public IVisualStudioBrowser Browser;

        [Import]
        public IExportFactoryProvider ExportFactoryProvider;

        [Import]
        public IGitHubServiceProvider GitHubServiceProvider;
        
        public Holder(ICompositionService cc)
        {
            cc.SatisfyImportsOnce(this);
        }
    }


    [Export(typeof(SVsServiceProvider))]
    [Export(typeof(SComponentModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CustomServiceProvider : SVsServiceProvider, IServiceProvider,
        SComponentModel, IComponentModel
    {
        readonly CompositionContainer container;
        public CompositionContainer Container { get { return container; } }
        AggregateCatalog catalog;

        public CustomServiceProvider()
        {
            catalog = new AggregateCatalog(
                new AssemblyCatalog(typeof(CustomServiceProvider).Assembly),
                new AssemblyCatalog(typeof(Program).Assembly), // GitHub.VisualStudio
                new AssemblyCatalog(typeof(GitHub.Api.ApiClient).Assembly), // GitHub.App
                new AssemblyCatalog(typeof(GitHub.Api.SimpleApiClient).Assembly), // GitHub.Api
                new AssemblyCatalog(typeof(Rothko.Environment).Assembly), // Rothko
                new AssemblyCatalog(typeof(EnterpriseProbeTask).Assembly) // GitHub.Exports
            );
            container = new CompositionContainer(catalog, CompositionOptions.IsThreadSafe | CompositionOptions.DisableSilentRejection);

            DefaultCatalog = catalog;
            DefaultExportProvider = container;
            DefaultCompositionService = DefaultCatalog.CreateCompositionService();
            
            var batch = new CompositionBatch();
            batch.AddExportedValue<SVsServiceProvider>(this);
            batch.AddExportedValue<SComponentModel>(this);
            batch.AddExportedValue<ICompositionService>(DefaultCompositionService);
            container.Compose(batch);
        }

        public object GetService(Type serviceType)
        {
            string contract = AttributedModelServices.GetContractName(serviceType);
            var instance = container.GetExportedValues<object>(contract).FirstOrDefault();

            if (instance != null)
                return instance;

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

        static object Create(Type t)
        {
            var moq = typeof(Mock<>).MakeGenericType(t);
            var ctor = moq.GetConstructor(new Type[] { });
            var m = ctor?.Invoke(new object[] { }) as Mock;
            return m?.Object;
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
