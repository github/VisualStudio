using GitHub.Infrastructure;
using ReactiveUI;
using Splat;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Windows;

namespace GitHub.VisualStudio
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UIProvider
    {
        [Import(typeof(IServiceProvider))]
        public MefServiceProvider ServiceProvider { get; set; }

        public UIProvider()
        {
            ModeDetector.OverrideModeDetector(new AppModeDetector());
            RxApp.MainThreadScheduler = new DispatcherScheduler(Application.Current.Dispatcher);
        }

        public void EnsureProvider(ExportProvider provider)
        {
            if (ServiceProvider.ExportProvider == null)
                ServiceProvider.ExportProvider = provider;
        }
    }

    [Export(typeof(IServiceProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MefServiceProvider : IServiceProvider
    {
        public ExportProvider ExportProvider { get; set; }

        public object GetService(Type serviceType)
        {
            string contract = AttributedModelServices.GetContractName(serviceType);
            var instance = ExportProvider.GetExportedValues<object>(contract).FirstOrDefault();

            if (instance != null)
                return instance;

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Could not locate any instances of contract {0}.", contract));
        }
    }
}
