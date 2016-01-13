using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
    [Guid(GuidList.guidGitHubPkgString)]
    //[ProvideBindingPath]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //[ProvideAutoLoad(UIContextGuids.NoSolution)]
    [ProvideAutoLoad("11B8E6D7-C08B-4385-B321-321078CDD1F8")]
    [ProvideToolWindow(typeof(GitHubPane), Orientation = ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindSolutionExplorer)]
    public class GitHubPackage : PackageBase
    {
        public GitHubPackage()
        {
        }

        public GitHubPackage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }


        protected override void Initialize()
        {
            base.Initialize();

            var menus = ServiceProvider.GetExportedValue<IMenuProvider>();
            foreach (var menu in menus.Menus)
                ServiceProvider.AddTopLevelMenuItem(menu.Guid, menu.CmdId, (s, e) => menu.Activate());

            foreach (var menu in menus.DynamicMenus)
                ServiceProvider.AddDynamicMenuItem(menu.Guid, menu.CmdId, menu.CanShow, menu.Activate);

/*
            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubCmdSet, PkgCmdIDList.addConnectionCommand, (s, e) => StartFlow(UIControllerFlow.Authentication));
            ServiceProvider.AddDynamicMenuItem(GuidList.guidContextMenuSet, PkgCmdIDList.createGistCommand, IsTextSelected, () =>
            {
                var activeRepo = ServiceProvider.GetExportedValue<ITeamExplorerServiceHolder>().ActiveRepo;
                // activeRepo can be null at this point because it is set elsewhere as the result of async operation that may not have completed yet.
                if (activeRepo == null)
                {
                    var vsservices = ServiceProvider.GetExportedValue<IVSServices>();
                    var path = vsservices?.GetActiveRepoPath() ?? string.Empty;
                    try
                    {
                        activeRepo = !string.IsNullOrEmpty(path) ? new SimpleRepositoryModel(path) : null;
                    }
                    catch (Exception ex)
                    {
                        VsOutputLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error loading the repository from '{0}'. {1}", path, ex));
                    }
                }
                
                var connections = ServiceProvider.GetExportedValue<IConnectionManager>().Connections;
                var connection = connections
                    .FirstOrDefault(c => activeRepo?.CloneUrl?.RepositoryName != null && c.HostAddress.Equals(HostAddress.Create(activeRepo.CloneUrl)));

                StartFlow(UIControllerFlow.Gist, connection);
            });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubCmdSet, PkgCmdIDList.showGitHubPaneCommand, (s, e) =>
            {
                var window = FindToolWindow(typeof(GitHubPane), 0, true);
                if (window?.Frame == null)
                    throw new NotSupportedException("Cannot create tool window");

                var windowFrame = (IVsWindowFrame)window.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            });
            base.Initialize();
        }

        bool IsTextSelected()
        {
            var selectedText = ServiceProvider.GetExportedValue<ISelectedTextProvider>();
            return selectedText.GetSelectedText().IsNotNullOrEmptyOrWhiteSpace();
        }

        void StartFlow(UIControllerFlow controllerFlow, IConnection connection = null)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.RunUI(controllerFlow, connection);
*/
        }
    }
}
