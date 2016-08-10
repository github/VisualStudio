using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Octokit;
using GitHub.Helpers;

namespace GitHub.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidGitHubPkgString)]
    //[ProvideBindingPath]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //[ProvideAutoLoad(UIContextGuids.NoSolution)]
    // this is the Git service GUID, so we load whenever it loads
    [ProvideAutoLoad("11B8E6D7-C08B-4385-B321-321078CDD1F8")]
    [ProvideToolWindow(typeof(GitHubPane), Orientation = ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindSolutionExplorer)]
    [ProvideOptionPage(typeof(OptionsPage), "GitHub for Visual Studio", "General", 0, 0, supportsAutomation: true)]
    public class GitHubPackage : Package
    {

        readonly IServiceProvider serviceProvider;

        static GitHubPackage()
        {
            AssemblyResolver.InitializeAssemblyResolver();
        }

        public GitHubPackage()
        {
            serviceProvider = this;
        }

        public GitHubPackage(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override void Initialize()
        {
            base.Initialize();
            IncrementLaunchCount();

            var menus = serviceProvider.GetExportedValue<IMenuProvider>();
            foreach (var menu in menus.Menus)
                serviceProvider.AddCommandHandler(menu.Guid, menu.CmdId, (s, e) => menu.Activate());

            foreach (var menu in menus.DynamicMenus)
                serviceProvider.AddCommandHandler(menu.Guid, menu.CmdId, menu.CanShow, () => menu.Activate());
        }

        void IncrementLaunchCount()
        {
            var usageTracker = serviceProvider.GetExportedValue<IUsageTracker>();
            usageTracker.IncrementLaunchCount();
        }
    }

    [Export(typeof(IGitHubClient))]
    public class GHClient : GitHubClient
    {
        [ImportingConstructor]
        public GHClient(IProgram program)
            : base(program.ProductHeader)
        {
        }
    }
}
