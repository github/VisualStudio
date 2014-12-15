using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using GitHub.Authentication;
using GitHub.Infrastructure;
using GitHub.ViewModels;
using GitHub.VisualStudio.UI.Views;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using ReactiveUI;
using Splat;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidGitHubPkgString)]
    [ProvideBindingPath]
    public class GitHubPackage : Package
    {
        readonly IServiceProvider serviceProvider;

        // Set of assemblies we need to load early.
        static readonly IEnumerable<string> earlyLoadAssemblies = new[] {
            "Rothko.dll",
            "GitHub.App.dll",
            "GitHub.UI.Reactive.dll",
            "GitHub.UI.dll"
        };

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public GitHubPackage()
        {
            serviceProvider = this;
        }

        public GitHubPackage(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        protected override void Initialize()
        {
            Debug.WriteLine("Entering Initialize() of: {0}", ToString());
            base.Initialize();

            ModeDetector.OverrideModeDetector(new AppModeDetector());
            RxApp.MainThreadScheduler = new DispatcherScheduler(Application.Current.Dispatcher);

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(dir != null, "The Assembly location can't be null");
            foreach (var v in earlyLoadAssemblies) 
            {
                Assembly.LoadFile(Path.Combine(dir, v));
            }

            // Set the Export Provider
            var mefServiceProvider = GetExportedValue<IServiceProvider>() as MefServiceProvider;
            Debug.Assert(mefServiceProvider != null, "Service Provider can't be imported");
            var componentModel = (IComponentModel)(serviceProvider.GetService(typeof(SComponentModel)));
            mefServiceProvider.ExportProvider = componentModel.DefaultExportProvider;

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = serviceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (mcs != null)
            {
                // Login Command Menu Item
                AddTopLevelMenuItem(mcs, PkgCmdIDList.loginCommand, OnLoginCommand);

                // Create Issue Command Menu Item
                AddTopLevelMenuItem(mcs, PkgCmdIDList.createIssueCommand, OnCreateIssueCommand);
            }
        }

        static void AddTopLevelMenuItem(
            IMenuCommandService menuCommandService,
            uint packageCommandId,
            EventHandler eventHandler)
        {
            var menuCommandId = new CommandID(GuidList.guidGitHubCmdSet, (int)packageCommandId);
            var menuItem = new MenuCommand(eventHandler, menuCommandId);
            menuCommandService.AddCommand(menuItem);
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        static void OnCreateIssueCommand(object sender, EventArgs e)
        {
            var createIssueDialog = new CreateIssueDialog();
            createIssueDialog.ShowModal();
        }

        void OnLoginCommand(object sender, EventArgs e)
        {
            var loginControlViewModel = GetExportedValue<LoginControlViewModel>();

            var loginIssueDialog = new LoginCommandDialog(loginControlViewModel);
            loginIssueDialog.Show();
            loginControlViewModel.AuthenticationResults.Subscribe(result =>
            {
                if (result == AuthenticationResult.Success)
                    loginIssueDialog.Hide();
            });
        }

        T GetExportedValue<T>()
        {
            var componentModel = (IComponentModel)(serviceProvider.GetService(typeof(SComponentModel)));
            var exportProvider = componentModel.DefaultExportProvider;
            return exportProvider.GetExportedValue<T>();
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
