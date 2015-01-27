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
using GitHub.Exports;
using GitHub.VisualStudio.Services;

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
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile")]
        protected override void Initialize()
        {
            Debug.WriteLine("Entering Initialize() of: {0}", ToString());
            base.Initialize();

            // Set the Export Provider


            // Add our command handlers for menu (commands must exist in the .vsct file)
            // Login Command Menu Item

            AddTopLevelMenuItem(PkgCmdIDList.loginCommand, OnLoginCommand);

            // Create Issue Command Menu Item
            AddTopLevelMenuItem(PkgCmdIDList.createIssueCommand, OnCreateIssueCommand);
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        void OnCreateIssueCommand(object sender, EventArgs e)
        {
            var createIssueDialog = new CreateIssueDialog();
            createIssueDialog.ShowModal();
        }

        void OnLoginCommand(object sender, EventArgs e)
        {
            EnsureUIProvider();
            /*
            var mefServiceProvider = GetExportedValue<IServiceProvider>() as MefServiceProvider;
            Debug.Assert(mefServiceProvider != null, "Service Provider can't be imported");
            var componentModel = GetService<SComponentModel>() as IComponentModel;
            if (componentModel != null)
                mefServiceProvider.ExportProvider = componentModel.DefaultExportProvider;
            */

            //var r = GetExportedValue<ILoginDialog>();
            var factory = GetExportedValue<ExportFactoryProvider>().LoginViewModelFactory;
            var disposable = factory.CreateExport();
            var loginControlViewModel = disposable.Value;

            var loginIssueDialog = new LoginCommandDialog(loginControlViewModel);
            loginIssueDialog.Closed += (o,ev) => disposable.Dispose();
            loginControlViewModel.CancelEvt.Subscribe(x => loginIssueDialog.Close());

            loginIssueDialog.Show();
            
            loginControlViewModel.AuthenticationResults.Subscribe(result =>
            {
                if (result == AuthenticationResult.Success)
                {
                    loginIssueDialog.Close();
                }
            });
            
        }

    }
    

    
}
