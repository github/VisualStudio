using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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
    /// This is a globally registered service (see `GitHubPackage`).
    /// If you need to access this service via MEF, use the `IGitHubServiceProvider` type
    /// </summary>
    public class GitHubServiceProvider : IGitHubServiceProvider, IDisposable
    {
        class OwnedComposablePart
        {
            public object Owner { get; set; }
            public ComposablePart Part { get; set; }
        }

        static readonly ILogger log = LogManager.ForContext<GitHubServiceProvider>();
        readonly IServiceProviderPackage asyncServiceProvider;
        readonly Dictionary<string, OwnedComposablePart> tempParts;
        readonly Version currentVersion;
        List<IDisposable> disposables = new List<IDisposable>();
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

        public GitHubServiceProvider(IServiceProviderPackage asyncServiceProvider)
        {
            Guard.ArgumentNotNull(asyncServiceProvider, nameof(asyncServiceProvider));

            this.currentVersion = this.GetType().Assembly.GetName().Version;
            this.asyncServiceProvider = asyncServiceProvider;

            tempParts = new Dictionary<string, OwnedComposablePart>();
        }

        public async Task InitializeAsync()
        {
            if (initialized)
                return;

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

        void InitializeSync()
        {
            if (initialized)
                return;

            IComponentModel componentModel = asyncServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;

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

        private object TryGetServiceSync(Type serviceType)
        {
            InitializeSync();

            var contract = AttributedModelServices.GetContractName(serviceType);
            var instance = AddToDisposables(TempContainer.GetExportedValueOrDefault<object>(contract));
            if (instance != null)
                return instance;

            try
            {
                instance = asyncServiceProvider.GetService(serviceType);
                if (instance != null)
                    return instance;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading {ServiceType}", serviceType);
            }

            instance = GitServiceProvider?.GetService(serviceType);
            if (instance != null)
                return instance;

            return null;
        }

        public async Task<T> TryGetServiceAsync<T>() where T : class
        {
            await InitializeAsync();

            var serviceType = typeof(T);

            var contract = AttributedModelServices.GetContractName(serviceType);
            var instance = AddToDisposables(TempContainer.GetExportedValueOrDefault<object>(contract));
            if (instance != null)
                return (T)instance;

            try
            {
                instance = await asyncServiceProvider.GetServiceAsync(serviceType);
                if (instance != null)
                    return (T)instance;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading {ServiceType}", serviceType);
            }

            instance = GitServiceProvider?.GetService(serviceType);
            if (instance != null)
                return (T)instance;

            return null;
        }

        public async Task<T> TryGetServiceMainThread<T>() where T : class
        {
            await InitializeAsync();

            var serviceType = typeof(T);

            var contract = AttributedModelServices.GetContractName(serviceType);
            var instance = AddToDisposables(TempContainer.GetExportedValueOrDefault<object>(contract));
            if (instance != null)
                return (T)instance;

            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                instance = await asyncServiceProvider.GetServiceAsync(serviceType);
                if (instance != null)
                    return (T)instance;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading {ServiceType}", serviceType);
            }

            instance = GitServiceProvider?.GetService(serviceType);
            if (instance != null)
                return (T)instance;

            return null;
        }

        public object TryGetMEFComponent(Type serviceType)
        {
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));

            InitializeSync();

            var contract = AttributedModelServices.GetContractName(serviceType);
            var instance = AddToDisposables(TempContainer.GetExportedValueOrDefault<object>(contract));
            if (instance != null)
                return instance;

            instance = AddToDisposables(ExportProvider.GetExportedValues<object>(contract).FirstOrDefault(x => contract.StartsWith("github.", StringComparison.OrdinalIgnoreCase) ? x.GetType().Assembly.GetName().Version == currentVersion : true));

            if (instance != null)
                return instance;

            return null;
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return TryGetServiceSync(serviceType);
        }

        public T TryGetServiceSync<T>() where T : class
        {
            return (T)TryGetServiceSync(typeof(T));
        }

        public async Task<T1> TryGetServiceAsync<T, T1>() where T : class where T1 : class
        {
            T ret = await TryGetServiceAsync<T>();
            return ret as T1;
        }

        public T TryGetMEFComponent<T>() where T : class
        {
            return (T)TryGetMEFComponent(typeof(T));
        }

        public T1 TryGetMEFComponent<T, T1>() where T : class where T1 : class
        {
            return TryGetMEFComponent(typeof(T)) as T1;
        }

        public T GetMEFComponent<T>() where T : class
        {
            T instance = TryGetMEFComponent<T>();
            if (instance != null)
                return instance;

            string contract = AttributedModelServices.GetContractName(typeof(T));
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Could not locate any instances of contract {0}.", contract));
        }

        public T1 GetMEFComponent<T, T1>() where T : class where T1 : class
        {
            var instance = TryGetMEFComponent<T, T1>();
            if (instance != null)
                return instance;

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Could not locate any instances of <{0}, {1}>.", typeof(T), typeof(T1)));
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
                {
                    foreach (var disposable in disposables)
                    {
                        disposable.Dispose();
                    }
                }

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
