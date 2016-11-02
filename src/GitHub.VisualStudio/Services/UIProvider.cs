using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using GitHub.Infrastructure;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NLog;
using NullGuard;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// This is a thin MEF wrapper around the GitHubServiceProvider
    /// which is registered as a global VS service. This class just
    /// redirects every request to the actual service, and can be
    /// thrown away as soon as the caller is done (no state is kept)
    /// </summary>
    [Export(typeof(IUIProvider))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubProviderDispatcher : IUIProvider
    {
        readonly IUIProvider theRealProvider;

        [ImportingConstructor]
        public GitHubProviderDispatcher([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            theRealProvider = serviceProvider.GetService(typeof(IUIProvider)) as IUIProvider;
        }

        public ExportProvider ExportProvider { get { return theRealProvider.ExportProvider; } }

        public IServiceProvider GitServiceProvider
        {
            get
            {
                return theRealProvider.GitServiceProvider;
            }

            set
            {
                theRealProvider.GitServiceProvider = value;
            }
        }

        public void AddService(Type t, object owner, object instance)
        {
            theRealProvider.AddService(t, owner, instance);
        }

        public void AddService<T>(object owner, T instance)
        {
            theRealProvider.AddService<T>(owner, instance);
        }

        public object GetService(Type serviceType)
        {
            return theRealProvider.GetService(serviceType);
        }

        public IObservable<bool> ListenToCompletionState()
        {
            return theRealProvider.ListenToCompletionState();
        }

        public void RemoveService(Type t, object owner)
        {
            theRealProvider.RemoveService(t, owner);
        }

        public void RunUI()
        {
            theRealProvider.RunUI();
        }

        public void RunUI(UIControllerFlow controllerFlow, IConnection connection)
        {
            theRealProvider.RunUI(controllerFlow, connection);
        }

        public IObservable<LoadData> SetupUI(UIControllerFlow controllerFlow, IConnection connection)
        {
            return theRealProvider.SetupUI(controllerFlow, connection);
        }

        public object TryGetService(string typename)
        {
            return theRealProvider.TryGetService(typename);
        }

        public object TryGetService(Type t)
        {
            return theRealProvider.TryGetService(t);
        }

        public T TryGetService<T>() where T : class
        {
            return theRealProvider.TryGetService<T>();
        }
    }

    /// <summary>
    /// This is a globally registered service (see `GitHubPackage`).
    /// If you need to access this service via MEF, use the `IUIProvider` type
    /// </summary>
    internal class GitHubServiceProvider : IUIProvider, IDisposable
    {
        class OwnedComposablePart
        {
            public object Owner { get; set; }
            public ComposablePart Part { get; set; }
        }

        static readonly Logger log = LogManager.GetCurrentClassLogger();
        CompositeDisposable disposables = new CompositeDisposable();
        readonly IServiceProvider serviceProvider;
        readonly Dictionary<string, OwnedComposablePart> tempParts;
        ExportLifetimeContext<IUIController> currentUIFlow;
        readonly Version currentVersion;
        bool initializingLogging = false;

        public ExportProvider ExportProvider { get; private set; }

        CompositionContainer tempContainer;
        CompositionContainer TempContainer
        {
            get
            {
                if (tempContainer == null)
                {
                    tempContainer = AddToDisposables(new CompositionContainer(new ComposablePartExportProvider()
                    {
                        SourceProvider = ExportProvider
                    }));
                }
                return tempContainer;
            }
        }

        [AllowNull]
        public IServiceProvider GitServiceProvider { get; set; }

        public GitHubServiceProvider(IServiceProvider serviceProvider)
        {
            this.currentVersion = this.GetType().Assembly.GetName().Version;
            this.serviceProvider = serviceProvider;

            tempParts = new Dictionary<string, OwnedComposablePart>();
        }

        public async Task Initialize()
        {
            var asyncProvider = serviceProvider as IAsyncServiceProvider;
            IComponentModel componentModel = null;
            if (asyncProvider != null)
            {
                componentModel = await asyncProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            }

            else
            {
                componentModel = serviceProvider?.GetService(typeof(SComponentModel)) as IComponentModel;
            }

            Debug.Assert(componentModel != null, "Service of type SComponentModel not found");
            if (componentModel == null)
            {
                log.Error("Service of type SComponentModel not found");
                return;
            }

            ExportProvider = componentModel.DefaultExportProvider;
            if (ExportProvider == null)
            {
                log.Error("DefaultExportProvider could not be obtained.");
            }
        }

        [return: AllowNull]
        public object TryGetService(Type serviceType)
        {
            if (!initializingLogging && log.Factory.Configuration == null)
            {
                initializingLogging = true;
                try
                {
                    var logging = TryGetService(typeof(ILoggingConfiguration)) as ILoggingConfiguration;
                    logging.Configure();
                }
                catch
                {
#if DEBUG
                    throw;
#endif
                }
            }

            string contract = AttributedModelServices.GetContractName(serviceType);
            var instance = AddToDisposables(TempContainer.GetExportedValueOrDefault<object>(contract));
            if (instance != null)
                return instance;

            instance = AddToDisposables(ExportProvider.GetExportedValues<object>(contract).FirstOrDefault(x => contract.StartsWith("github.", StringComparison.OrdinalIgnoreCase) ? x.GetType().Assembly.GetName().Version == currentVersion : true));

            if (instance != null)
                return instance;

            instance = serviceProvider.GetService(serviceType);
            if (instance != null)
                return instance;

            instance = GitServiceProvider?.GetService(serviceType);
            if (instance != null)
                return instance;

            return null;
        }

        [return: AllowNull]
        public object TryGetService(string typename)
        {
            var type = Type.GetType(typename, false, true);
            return TryGetService(type);
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

        [return: AllowNull]
        public T TryGetService<T>() where T : class
        {
            return TryGetService(typeof(T)) as T;
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public Ret GetService<T, Ret>() where Ret : class
        {
            return GetService<T>() as Ret;
        }

        public void AddService<T>(object owner, T instance)
        {
            AddService(typeof(T), owner, instance);
        }

        public void AddService(Type t, object owner, object instance)
        {
            string contract = AttributedModelServices.GetContractName(t);
            Debug.Assert(!string.IsNullOrEmpty(contract), "Every type must have a contract name");

            // we want to remove stale instances of a service, if they exist, regardless of who put them there
            RemoveService(t, null);

            var batch = new CompositionBatch();
            var part = batch.AddExportedValue(contract, instance);
            Debug.Assert(part != null, "Adding an exported value must return a non-null part");
            tempParts.Add(contract, new OwnedComposablePart { Owner = owner, Part = part });
            TempContainer.Compose(batch);
        }

        /// <summary>
        /// Removes a service from the catalog
        /// </summary>
        /// <param name="t">The type we want to remove</param>
        /// <param name="owner">The owner, which either has to match what was passed to AddService,
        /// or if it's null, the service will be removed without checking for ownership</param>
        public void RemoveService(Type t, [AllowNull] object owner)
        {
            string contract = AttributedModelServices.GetContractName(t);
            Debug.Assert(!string.IsNullOrEmpty(contract), "Every type must have a contract name");

            OwnedComposablePart part; 
            if (tempParts.TryGetValue(contract, out part))
            {
                if (owner != null && part.Owner != owner)
                    return;
                tempParts.Remove(contract);
                var batch = new CompositionBatch();
                batch.RemovePart(part.Part);
                TempContainer.Compose(batch);
            }
        }

        UI.WindowController windowController;
        public IObservable<LoadData> SetupUI(UIControllerFlow controllerFlow, [AllowNull] IConnection connection)
        {
            StopUI();

            var factory = TryGetService(typeof(IExportFactoryProvider)) as IExportFactoryProvider;
            currentUIFlow = factory.UIControllerFactory.CreateExport();
            var disposable = currentUIFlow;
            var ui = currentUIFlow.Value;
            var creation = ui.SelectFlow(controllerFlow);
            windowController = new UI.WindowController(creation);
            windowController.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            windowController.Closed += StopUIFlowWhenWindowIsClosedByUser;
            creation.Subscribe(c => {}, () =>
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

        public IObservable<bool> ListenToCompletionState()
        {
            var ui = currentUIFlow?.Value;
            if (ui == null)
            {
                log.Error("UIProvider:ListenToCompletionState:Cannot call ListenToCompletionState without calling SetupUI first");
#if DEBUG
                throw new InvalidOperationException("Cannot call ListenToCompletionState without calling SetupUI first");
#endif
            }
            return ui?.ListenToCompletionState() ?? Observable.Return(false);
        }

        public void RunUI()
        {
            Debug.Assert(windowController != null, "WindowController is null, did you forget to call SetupUI?");
            if (windowController == null)
            {
                log.Error("WindowController is null, cannot run UI.");
                return;
            }
            try
            {
                windowController.ShowModal();
            }
            catch (Exception ex)
            {
                log.Error("WindowController ShowModal failed. {0}", ex);
            }
        }

        public void RunUI(UIControllerFlow controllerFlow, [AllowNull] IConnection connection)
        {
            SetupUI(controllerFlow, connection);
            try
            {
                windowController.ShowModal();
            }
            catch (Exception ex)
            {
                log.Error("WindowController ShowModal failed for {0}. {1}", controllerFlow, ex);
            }
        }

        public void StopUI()
        {
            StopUI(currentUIFlow);
            currentUIFlow = null;
        }

        static void StopUI(ExportLifetimeContext<IUIController> disposable)
        {
            try {
                if (disposable != null && disposable.Value != null)
                {
                    if (!disposable.Value.IsStopped)
                        disposable.Value.Stop();
                    disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed to dispose UI. {0}", ex);
            }
        }

        T AddToDisposables<T>(T instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposables.Add(disposable);
            }
            return instance;
        }

        void StopUIFlowWhenWindowIsClosedByUser(object sender, EventArgs e)
        {
            StopUI();
        }

        bool disposed;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                StopUI();
                if (disposables != null)
                    disposables.Dispose();
                disposables = null;
                if (tempContainer != null)
                    tempContainer.Dispose();
                tempContainer = null;
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
