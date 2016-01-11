using System;
using System.Globalization;
using System.Runtime.InteropServices;
using GitHub.Extensions;
using GitHub.Models;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
        protected override void Initialize()
        {
            base.Initialize();

			var menus = ServiceProvider.GetExportedValue<IMenuProvider>();
            foreach (var menu in menus.Menus)
                ServiceProvider.AddTopLevelMenuItem(menu.Guid, menu.CmdId, (s, e) => menu.Activate());

            foreach (var menu in menus.DynamicMenus)
                ServiceProvider.AddDynamicMenuItem(menu.Guid, menu.CmdId, menu.CanShow, menu.Activate);
            ServiceProvider.AddDynamicMenuItem(GuidList.guidContextMenuSet, PkgCmdIDList.getLinkCommand,
                IsValidGithubRepo, 
                OpenRepoInBrowser);
            ServiceProvider.AddDynamicMenuItem(GuidList.guidContextMenuSet, PkgCmdIDList.copyLinkCommand,
                IsValidGithubRepo,
                CopyRepoLinkToClipboard);
        }

        private void CopyRepoLinkToClipboard()
        {
            if (!IsValidGithubRepo()) return;

            var activeDocument = ServiceProvider.GetExportedValue<IActiveDocument>();
            var activeRepo = ServiceProvider.GetExportedValue<ITeamExplorerServiceHolder>().ActiveRepo;
            var outputUri = activeRepo?.BrowserUrl(activeDocument);

            if (string.IsNullOrEmpty(outputUri)) return;

            System.Windows.Clipboard.SetText(outputUri);
        }

        private bool IsValidGithubRepo()
        {
            var cloneUrl = ServiceProvider.GetExportedValue<ITeamExplorerServiceHolder>().ActiveRepo?.CloneUrl;

            if (string.IsNullOrEmpty(cloneUrl))
                return false;

            return cloneUrl.Host == "github.com";
        }

        private void OpenRepoInBrowser()
        {
            if (!IsValidGithubRepo()) return;

            var activeDocument = ServiceProvider.GetExportedValue<IActiveDocument>();
            var activeRepo = ServiceProvider.GetExportedValue<ITeamExplorerServiceHolder>().ActiveRepo;

            var outputUri = activeRepo?.BrowserUrl(activeDocument);

            if (string.IsNullOrEmpty(outputUri)) return;

            var vsBrowserProvider = ServiceProvider.GetExportedValue<IVisualStudioBrowser>();
            vsBrowserProvider.OpenUrl(new Uri(outputUri));
        }

    }
}
