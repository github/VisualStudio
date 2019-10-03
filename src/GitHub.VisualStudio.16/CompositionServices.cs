using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.Views.Dialog.Clone;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Rothko;
using static Microsoft.VisualStudio.Composition.NetFxAdapters;
using ExportProvider = System.ComponentModel.Composition.Hosting.ExportProvider;

namespace GitHub.VisualStudio
{
    [Export]
    public class CompositionServices
    {
        readonly ExportProvider defaultExportProvider;
        ExportProvider exportProvider;

        public CompositionServices()
        {
        }

        [ImportingConstructor]
        public CompositionServices(Microsoft.VisualStudio.Composition.ExportProvider defaultExportProvider)
        {
            this.defaultExportProvider = defaultExportProvider.AsExportProvider();
        }

        public ExportProvider GetExportProvider()
        {
            return exportProvider = exportProvider ?? CreateCompositionContainer();
        }

        CompositionContainer CreateCompositionContainer()
        {
            if (defaultExportProvider is ExportProvider exportProvider)
            {
                return CreateVisualStudioCompositionContainer(exportProvider);
            }

            return CreateOutOfProcCompositionContainer();
        }

        static CompositionContainer CreateVisualStudioCompositionContainer(ExportProvider defaultExportProvider)
        {
            var compositionContainer = CreateCompositionContainer(defaultExportProvider);
            var usageTracker = CreateUsageTracker(compositionContainer);
            compositionContainer.ComposeExportedValue(usageTracker);

            return compositionContainer;
        }

        static CompositionContainer CreateOutOfProcCompositionContainer()
        {
            var compositionContainer = CreateCompositionContainer(CreateOutOfProcExports());
            var usageTracker = CreateUsageTracker(compositionContainer);
            compositionContainer.ComposeExportedValue(usageTracker);

            return compositionContainer;
        }

        static IUsageTracker CreateUsageTracker(CompositionContainer compositionContainer)
        {
            var connectionManager = compositionContainer.GetExport<IConnectionManager>();
            return new VisualStudioUsageTracker(connectionManager);
        }

        static CompositionContainer CreateOutOfProcExports()
        {
            var container = new CompositionContainer();

            var serviceProvider = new OutOfProcSVsServiceProvider();
            container.ComposeExportedValue<SVsServiceProvider>(serviceProvider);

#pragma warning disable VSSDK005 // Avoid instantiating JoinableTaskContext
            var joinableTaskContext = new JoinableTaskContext();
#pragma warning restore VSSDK005 // Avoid instantiating JoinableTaskContext
            container.ComposeExportedValue(joinableTaskContext);

            return container;
        }

        static CompositionContainer CreateCompositionContainer(ExportProvider defaultExportProvider)
        {
            var catalog = new LoggingCatalog(
                GetCatalog(typeof(DialogService).Assembly), // GitHub.App
                GetCatalog(typeof(GraphQLClientFactory).Assembly), // GitHub.Api
                GetCatalog(typeof(RepositoryCloneView).Assembly), // GitHub.VisualStudio.UI
                GetCatalog(typeof(GitHubPackage).Assembly), // GitHub.VisualStudio
                GetCatalog(typeof(VSGitServices).Assembly), // GitHub.TeamFoundation.16
                GetCatalog(typeof(GitService).Assembly), // GitHub.Exports
                GetCatalog(typeof(NotificationDispatcher).Assembly), // GitHub.Exports.Reactive          
                GetCatalog(typeof(IOperatingSystem).Assembly) // Rothko
            );

            var compositionContainer = new CompositionContainer(catalog, defaultExportProvider);

            var gitHubServiceProvider = new MyGitHubServiceProvider(compositionContainer);
            compositionContainer.ComposeExportedValue<IGitHubServiceProvider>(gitHubServiceProvider);
            Services.UnitTestServiceProvider = gitHubServiceProvider; // Use gitHubServiceProvider as global provider 

            var loginManager = LoginManagerFactory.CreateLoginManager(compositionContainer);
            compositionContainer.ComposeExportedValue(loginManager);

            // Ensure GitHub.Resources.dll has been loaded and it visible to XAML
            EnsureLoaded(typeof(GitHub.Resources));

            return compositionContainer;
        }

        static void EnsureLoaded(Type type)
        {
            // Ensure the containing assembly has been loaded
            Assumes.NotNull(type);
        }

        class LoginManagerFactory
        {
            internal static ILoginManager CreateLoginManager(CompositionContainer compositionContainer)
            {
                var keychain = compositionContainer.GetExportedValue<IKeychain>();
                var lazy2Fa = new Lazy<ITwoFactorChallengeHandler>(() => compositionContainer.GetExportedValue<ITwoFactorChallengeHandler>());
                var oauthListener = compositionContainer.GetExportedValue<IOAuthCallbackListener>();
                var loginManager = new LoginManager(
                        keychain,
                        lazy2Fa,
                        oauthListener,
                        ApiClientConfiguration.ClientId,
                        ApiClientConfiguration.ClientSecret,
                        ApiClientConfiguration.MinimumScopes,
                        ApiClientConfiguration.RequestedScopes,
                        ApiClientConfiguration.AuthorizationNote,
                        ApiClientConfiguration.MachineFingerprint);
                return loginManager;
            }
        }

        static TypeCatalog GetCatalog(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Debug.WriteLine(e);
                foreach (var ex in e.LoaderExceptions)
                {
                    Debug.WriteLine(ex);
                }

                types = e.Types.Where(t => t != null).ToArray();
            }

            var catalog = new TypeCatalog(types);
            return catalog;
        }
    }

    public class MyGitHubServiceProvider : IGitHubServiceProvider
    {
        readonly IServiceProvider serviceProvider;

        public MyGitHubServiceProvider(ExportProvider exportProvider)
        {
            ExportProvider = exportProvider;
            serviceProvider = exportProvider.GetExportedValue<SVsServiceProvider>();
        }

        public T TryGetService<T>() where T : class
        {
            try
            {
                return GetService<T>();
            }
            catch
            {
                return default;
            }
        }

        public T GetService<T>() where T : class
        {
            return GetService<T, T>();
        }

        public TRet GetService<T, TRet>()
            where T : class
            where TRet : class
        {
            var value = ExportProvider.GetExportedValueOrDefault<T>();
            if (value != null)
            {
                return value as TRet;
            }

            value = GetService(typeof(T)) as T;
            if (value != null)
            {
                return value as TRet;
            }

            Debug.WriteLine($"Couldn't find service of type {typeof(T)}");
            return null;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IGitHubServiceProvider))
            {
                return this;
            }

            return serviceProvider.GetService(serviceType);
        }

        #region obsolete

        public IServiceProvider GitServiceProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddService(Type t, object owner, object instance)
        {
            throw new NotImplementedException();
        }

        public void AddService<T>(object owner, T instance) where T : class
        {
            throw new NotImplementedException();
        }

        public void RemoveService(Type t, object owner)
        {
            throw new NotImplementedException();
        }

        public object TryGetService(Type t)
        {
            throw new NotImplementedException();
        }

        public object TryGetService(string typeName)
        {
            throw new NotImplementedException();
        }

        #endregion

        public ExportProvider ExportProvider { get; }
    }

    class OutOfProcSVsServiceProvider : SVsServiceProvider
    {
        public object GetService(Type serviceType)
        {
            Debug.WriteLine($"GetService: {serviceType}");
            return null;
        }
    }

    public class LoggingCatalog : AggregateCatalog
    {
        public LoggingCatalog(params ComposablePartCatalog[] catalogs) : base(catalogs) { }

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            var exports = base.GetExports(definition);
            if (exports.Count() == 0)
            {
                Debug.WriteLine($"No exports for {definition}");
            }

            return exports;
        }
    }
}
