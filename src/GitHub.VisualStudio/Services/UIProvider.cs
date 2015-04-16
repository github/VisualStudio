using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NullGuard;

namespace GitHub.VisualStudio
{
    [Export(typeof(IUIProvider))]
    [Export(typeof(IServiceProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class UIProvider : IServiceProvider, IUIProvider, IDisposable
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
        readonly CompositionContainer tempContainer;
        readonly Dictionary<string, ComposablePart> tempParts;
        ExportLifetimeContext<IUIController> currentUIFlow;

        [ImportingConstructor]
        public UIProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            var componentModel = serviceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Debug.Assert(componentModel != null, "Service of type SComponentModel not found");
            ExportProvider = componentModel.DefaultExportProvider;

            tempContainer = new CompositionContainer(new ComposablePartExportProvider() { SourceProvider = ExportProvider });
            tempParts = new Dictionary<string, ComposablePart>();
        }

        [return: AllowNull]
        public object TryGetService(Type serviceType)
        {
            string contract = AttributedModelServices.GetContractName(serviceType);
            var instance = tempContainer.GetExportedValueOrDefault<object>(contract);
            if (instance != null)
                return instance;

            instance = ExportProvider.GetExportedValues<object>(contract).FirstOrDefault();

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

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public Ret GetService<T, Ret>() where Ret : class
        {
            return GetService<T>() as Ret;
        }

        public void AddService(Type t, object instance)
        {
            var batch = new CompositionBatch();
            string contract = AttributedModelServices.GetContractName(t);
            Debug.Assert(!string.IsNullOrEmpty(contract), "Every type must have a contract name");
            var part = batch.AddExportedValue(contract, instance);
            Debug.Assert(part != null, "Adding an exported value must return a non-null part");
            tempParts.Add(contract, part);
            tempContainer.Compose(batch);
        }

        public void RemoveService(Type t)
        {
            string contract = AttributedModelServices.GetContractName(t);
            Debug.Assert(!string.IsNullOrEmpty(contract), "Every type must have a contract name");

            ComposablePart part; 

            if (tempParts.TryGetValue(contract, out part))
            {
                tempParts.Remove(contract);
                var batch = new CompositionBatch();
                batch.RemovePart(part);
                tempContainer.Compose(batch);
            }
        }

        UI.WindowController windowController;
        public IObservable<object> SetupUI(UIControllerFlow controllerFlow, [AllowNull] IConnection connection)
        {
            StopUI();

            var factory = GetService<IExportFactoryProvider>();
            currentUIFlow = factory.UIControllerFactory.CreateExport();
            var disposable = currentUIFlow;
            var ui = currentUIFlow.Value;
            var creation = ui.SelectFlow(controllerFlow);
            windowController = new UI.WindowController(creation);
            windowController.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            windowController.Closed += StopUIFlowWhenWindowIsClosedByUser;
            creation.Subscribe((c) => {}, () =>
            {
                windowController.Closed -= StopUIFlowWhenWindowIsClosedByUser;
                windowController.Close();
                if (currentUIFlow != disposable)
                    StopUI(disposable);
                else
                    StopUI();
            });
            ui.Start(connection);
            return creation;
        }

        public void RunUI()
        {
            Debug.Assert(windowController != null, "WindowController is null, did you forget to call SetupUI?");
            if (windowController == null)
                return;
            windowController.ShowModal();
        }

        public void RunUI(UIControllerFlow controllerFlow, [AllowNull] IConnection connection)
        {
            SetupUI(controllerFlow, connection);
            windowController.ShowModal();
        }

        public void StopUI()
        {
            StopUI(currentUIFlow);
            currentUIFlow = null;
        }

        static void StopUI(ExportLifetimeContext<IUIController> disposable)
        {
            if (disposable != null)
            {
                if (!disposable.Value.IsStopped)
                    disposable.Value.Stop();
                disposable.Dispose();
            }
        }

        void StopUIFlowWhenWindowIsClosedByUser(object sender, EventArgs e)
        {
            StopUI();
        }

        bool disposed = false;
        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopUI();
                    if (tempContainer != null)
                    {
                        tempContainer.Dispose();
                    }
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
