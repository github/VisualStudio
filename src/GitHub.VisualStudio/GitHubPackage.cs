using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using GitHub.VisualStudio.UI.Views;
using Microsoft.VisualStudio.Shell;
using GitHub.Exports;
using GitHub.Services;
using GitHub.Authentication;

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
    public class GitHubPackage : PackageBase
    {
        public GitHubPackage()
        {
        }

        public GitHubPackage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine("Entering Initialize() of: {0}", ToString());
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            // for testing purposes only
            AddTopLevelMenuItem(PkgCmdIDList.loginCommand, OnLoginCommand);
            AddTopLevelMenuItem(PkgCmdIDList.createRepoCommand, OnCreateRepo);
            AddTopLevelMenuItem(PkgCmdIDList.cloneRepoCommand, OnCloneRepo);
        }

        void ShowDialog(GitHub.UI.UIControllerFlow flow)
        {
            var ui = GetExportedValue<IUIProvider>();
            var disposable = ui.GetService<ExportFactoryProvider>().UIControllerFactory.CreateExport();
            var watcher = disposable.Value.SelectFlow(flow);
            var window = new WindowController(watcher);
            watcher.Subscribe(_ => { }, _ => {
                window.Close();
                disposable.Dispose();
            });
            //window.Owner = System.Windows.Application.Current.MainWindow;
            //window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            window.ShowModal();
        }

        void OnCreateRepo(object sender, EventArgs e)
        {
            ShowDialog(GitHub.UI.UIControllerFlow.Create);
        }

        void OnCloneRepo(object sender, EventArgs e)
        {
            ShowDialog(GitHub.UI.UIControllerFlow.Clone);
        }

        void OnLoginCommand(object sender, EventArgs e)
        {
            ShowDialog(GitHub.UI.UIControllerFlow.Authentication);
        }
    }
}
