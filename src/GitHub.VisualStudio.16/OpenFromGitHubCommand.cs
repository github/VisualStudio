using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using GitHub.Api;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio.UI.Services;
using GitHub.VisualStudio.Views;
using GitHub.VisualStudio.Views.Dialog.Clone;
using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Rothko;
using Task = System.Threading.Tasks.Task;

namespace GitHubCore
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenFromGitHubCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("aa0f1730-6a63-4ae3-9b61-3a1f34761663");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFromGitHubCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenFromGitHubCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenFromGitHubCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get => package;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in OpenFromGitHubCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenFromGitHubCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            Assumes.Present(dte);
            try
            {
                dte.ExecuteCommand("GitHub.OpenFromUrl");
            }
            catch(COMException)
            {
                var componentModel = (IComponentModel)ServiceProvider.GetService(typeof(SComponentModel));
                Assumes.Present(componentModel);
                ShowCloneDialogAsync(componentModel.DefaultExportProvider).FileAndForget("ShowCloneDialogAsync");
            }
        }

        [STAThread]
        static async Task ShowCloneDialogAsync(ExportProvider defaultExportProvider)
        {
            defaultExportProvider = defaultExportProvider ?? CreateOutOfProcExports();

            var catalog = new LoggingCatalog(
                GetCatalog(typeof(DialogService).Assembly), // GitHub.App
                GetCatalog(typeof(GraphQLClientFactory).Assembly), // GitHub.Api
                GetCatalog(typeof(RepositoryCloneView).Assembly), // GitHub.VisualStudio.UI
                GetCatalog(typeof(ShowDialogService).Assembly), // GitHub.VisualStudio
                GetCatalog(typeof(VSGitServices).Assembly), // GitHub.TeamFoundation.16
                GetCatalog(typeof(GitService).Assembly), // GitHub.Exports
                GetCatalog(typeof(NotificationDispatcher).Assembly), // GitHub.Exports.Reactive          
                GetCatalog(typeof(IOperatingSystem).Assembly) // Rothko
            );

            var compositionContainer = new CompositionContainer(catalog, defaultExportProvider);

            var gitHubServiceProvider = new MyGitHubServiceProvider(compositionContainer);
            compositionContainer.ComposeExportedValue<IGitHubServiceProvider>(gitHubServiceProvider);

            var usageTracker = new MyUsageTracker();
            compositionContainer.ComposeExportedValue<IUsageTracker>(usageTracker);

            var loginManager = CreateLoginManager(compositionContainer);
            compositionContainer.ComposeExportedValue<ILoginManager>(loginManager);

            var dialogService = compositionContainer.GetExportedValue<IDialogService>();
            var repositoryCloneService = compositionContainer.GetExportedValue<IRepositoryCloneService>();

            var viewViewModelFactory = compositionContainer.GetExportedValue<IViewViewModelFactory>();
            using (new ViewLocatorInitializer(viewViewModelFactory))
            {
                var url = null as string;
                var cloneDialogResult = await dialogService.ShowCloneDialog(null, url);
                if (cloneDialogResult != null)
                {
                    await repositoryCloneService.CloneOrOpenRepository(cloneDialogResult);
                }
            }
        }

        private static LoginManager CreateLoginManager(CompositionContainer compositionContainer)
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

        static CompositionContainer CreateOutOfProcExports()
        {
            var container = new CompositionContainer();
            var serviceProvider = new MySVsServiceProvider();
            container.ComposeExportedValue<SVsServiceProvider>(serviceProvider);
            return container;
        }

        class MySVsServiceProvider : SVsServiceProvider
        {
            public object GetService(Type serviceType)
            {
                Console.WriteLine($"GetService: {serviceType}");
                return null;
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
                Trace.WriteLine(e);
                foreach (var ex in e.LoaderExceptions)
                {
                    Trace.WriteLine(ex);
                }

                types = e.Types.Where(t => t != null).ToArray();
            }

            var catalog = new TypeCatalog(types);
            return catalog;
        }
    }

    public class ViewLocatorInitializer : IDisposable
    {
        object value;

        public ViewLocatorInitializer(IViewViewModelFactory viewViewModelFactory)
        {
            value = FactoryProviderFiled().GetValue(null);
            FactoryProviderFiled().SetValue(null, viewViewModelFactory);
        }

        public void Dispose()
        {
            FactoryProviderFiled().SetValue(null, value);
        }

        private static FieldInfo FactoryProviderFiled()
        {
            return typeof(ViewLocator).GetField("factoryProvider", BindingFlags.Static | BindingFlags.NonPublic);
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

            Trace.WriteLine($"Couldn't find service of type {typeof(T)}");
            return null;
        }

        public object GetService(Type serviceType)
        {
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

    public class MyUsageTracker : IUsageTracker
    {
        public Task IncrementCounter(Expression<Func<UsageModel.MeasuresModel, int>> counter)
        {
            Trace.WriteLine($"IncrementCounter {counter}");
            return Task.CompletedTask;
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
                Trace.WriteLine($"No exports for {definition}");
            }

            return exports;
        }
    }
}
