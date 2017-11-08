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
using GitHub.Logging;
using GitHub.Models;
using GitHub.Exports;
using GitHub.Services;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;
using GitHub.Extensions;
using Serilog;
using Log = GitHub.Logging.Log;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// This is a thin MEF wrapper around the GitHubServiceProvider
    /// which is registered as a global VS service. This class just
    /// redirects every request to the actual service, and can be
    /// thrown away as soon as the caller is done (no state is kept)
    /// </summary>
    [ExportForProcess(typeof(IGitHubServiceProvider), "devenv")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHubProviderDispatcher : IGitHubServiceProvider
    {
        readonly IGitHubServiceProvider theRealProvider;

        [ImportingConstructor]
        public GitHubProviderDispatcher([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            theRealProvider = serviceProvider.GetService(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;
        }

        public ExportProvider ExportProvider => theRealProvider.ExportProvider;

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

        public void AddService(Type t, object owner, object instance) => theRealProvider.AddService(t, owner, instance);

        public void AddService<T>(object owner, T instance) where T : class => theRealProvider.AddService(owner, instance);

        public T GetService<T>() where T : class => theRealProvider.GetService<T>();

        public object GetService(Type serviceType) => theRealProvider.GetService(serviceType);

        public Ret GetService<T, Ret>() where T : class where Ret : class => theRealProvider.GetService<T, Ret>();

        public void RemoveService(Type t, object owner) => theRealProvider.RemoveService(t, owner);

        public object TryGetService(string typename) => theRealProvider.TryGetService(typename);

        public object TryGetService(Type t) => theRealProvider.TryGetService(t);

        public T TryGetService<T>() where T : class => theRealProvider.TryGetService<T>();
    }

    /// <summary>
    /// This is a globally registered service (see `GitHubPackage`).
    /// If you need to access this service via MEF, use the `IGitHubServiceProvider` type
    /// </summary>
    public class GitHubServiceProvider : IGitHubServiceProvider, IDisposable
    {
        public static IGitHubServiceProvider Instance => Package.GetGlobalService(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;

        class OwnedComposablePart
        {
            public object Owner { get; set; }
            public ComposablePart Part { get; set; }
        }

        static readonly ILogger log = LogManager.ForContext<GitHubServiceProvider>();
        CompositeDisposable disposables = new CompositeDisposable();
        readonly IServiceProviderPackage asyncServiceProvider;
        readonly IServiceProvider syncServiceProvider;
        readonly Dictionary<string, OwnedComposablePart> tempParts;
        readonly Version currentVersion;
        bool initialized = false;

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

        public IServiceProvider GitServiceProvider { get; set; }

        public GitHubServiceProvider(IServiceProviderPackage asyncServiceProvider, IServiceProvider syncServiceProvider)
        {
            Guard.ArgumentNotNull(asyncServiceProvider, nameof(asyncServiceProvider));
            Guard.ArgumentNotNull(syncServiceProvider, nameof(syncServiceProvider));

            this.currentVersion = this.GetType().Assembly.GetName().Version;
            this.asyncServiceProvider = asyncServiceProvider;
            this.syncServiceProvider = syncServiceProvider;

            tempParts = new Dictionary<string, OwnedComposablePart>();
        }

        public async Task Initialize()
        {
            IComponentModel componentModel = await asyncServiceProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;

            Log.Assert(componentModel != null, "Service of type SComponentModel not found");
            if (componentModel == null)
            {
                log.Error("Service of type SComponentModel not found");
                return;
            }

            ExportProvider = componentModel.DefaultExportProvider;
            if (ExportProvider == null)
            {
                log.Error("DefaultExportProvider could not be obtained");
            }
            initialized = true;
        }


        public object TryGetService(Type serviceType)
        {
            string contract = AttributedModelServices.GetContractName(serviceType);
            var instance = AddToDisposables(TempContainer.GetExportedValueOrDefault<object>(contract));
            if (instance != null)
                return instance;

            var sp = initialized ? syncServiceProvider : asyncServiceProvider;

            instance = sp.GetService(serviceType);
            if (instance != null)
                return instance;

            instance = AddToDisposables(ExportProvider.GetExportedValues<object>(contract).FirstOrDefault(x => contract.StartsWith("github.", StringComparison.OrdinalIgnoreCase) ? x.GetType().Assembly.GetName().Version == currentVersion : true));

            if (instance != null)
                return instance;

            instance = GitServiceProvider?.GetService(serviceType);
            if (instance != null)
                return instance;

            return null;
        }

        public object TryGetService(string typename)
        {
            Guard.ArgumentNotEmptyString(typename, nameof(typename));

            var type = Type.GetType(typename, false, true);
            return TryGetService(type);
        }

        public object GetService(Type serviceType)
        {
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));

            var instance = TryGetService(serviceType);
            if (instance != null)
                return instance;

            string contract = AttributedModelServices.GetContractName(serviceType);
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Could not locate any instances of contract {0}.", contract));
        }

        public T GetService<T>() where T : class
        {
            return (T)GetService(typeof(T));
        }

        public T TryGetService<T>() where T : class
        {
            return TryGetService(typeof(T)) as T;
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public Ret GetService<T, Ret>() where T : class
                                        where Ret : class
        {
            return TryGetService(typeof(T)) as Ret;
        }

        public void AddService<T>(object owner, T instance) where T : class
        {
            Guard.ArgumentNotNull(owner, nameof(owner));
            Guard.ArgumentNotNull(instance, nameof(instance));

            AddService(typeof(T), owner, instance);
        }

        public void AddService(Type t, object owner, object instance)
        {
            Guard.ArgumentNotNull(t, nameof(t));
            Guard.ArgumentNotNull(owner, nameof(owner));
            Guard.ArgumentNotNull(instance, nameof(instance));

            string contract = AttributedModelServices.GetContractName(t);

            if (string.IsNullOrEmpty(contract))
            {
                throw new GitHubLogicException("Every type must have a contract name");
            }

            // we want to remove stale instances of a service, if they exist, regardless of who put them there
            RemoveService(t, null);

            var batch = new CompositionBatch();
            var part = batch.AddExportedValue(contract, instance);

            if (part == null)
            {
                throw new GitHubLogicException("Adding an exported value must return a non-null part");
            }

            tempParts.Add(contract, new OwnedComposablePart { Owner = owner, Part = part });
            TempContainer.Compose(batch);
        }

        /// <summary>
        /// Removes a service from the catalog
        /// </summary>
        /// <param name="t">The type we want to remove</param>
        /// <param name="owner">The owner, which either has to match what was passed to AddService,
        /// or if it's null, the service will be removed without checking for ownership</param>
        public void RemoveService(Type t, object owner)
        {
            Guard.ArgumentNotNull(t, nameof(t));

            string contract = AttributedModelServices.GetContractName(t);
            Log.Assert(!string.IsNullOrEmpty(contract), "Every type must have a contract name");

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

        T AddToDisposables<T>(T instance) where T : class
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposables.Add(disposable);
            }
            return instance;
        }

        bool disposed;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

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
