using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Octokit;
using GitHub.Helpers;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Threading;
using tasks = System.Threading.Tasks;

namespace GitHub.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidGitHubPkgString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
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

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideService(typeof(IUIProvider), IsAsyncQueryable = true)]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [Guid(ServiceProviderPackageId)]
    public sealed class ServiceProviderPackage : AsyncPackage
    {
        const string ServiceProviderPackageId = "D5CE1488-DEDE-426D-9E5B-BFCCFBE33E53";
        const string StartPagePreview4PackageId = "3b764d23-faf7-486f-94c7-b3accc44a70d";

        Version vsversion;
        Version VSVersion
        {
            get
            {
                if (vsversion == null)
                {
                    var asm = typeof(ITaskList).Assembly;
                    try
                    {
                        // this will return Microsoft.VisualStudio.Shell.Immutable.14.0 in VS15
                        // but Microsoft.VisualStudio.Shell.Framework in Dev15
                        var vinfo = FileVersionInfo.GetVersionInfo(asm.Location);
                        vsversion = new Version(vinfo.FileMajorPart, vinfo.FileMinorPart, vinfo.FileBuildPart, vinfo.FilePrivatePart);
                    }
                    catch
                    {
                        // something wrong, fallback to assembly version
                        vsversion = asm.GetName().Version;
                    }
                }
                return vsversion;
            }
        }

        protected override async tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            AddService(typeof(IUIProvider), CreateService, true);

            // Load the start page package only for Dev15 Preview 4
            if (VSVersion.Major == 15 && VSVersion.Build == 25618)
            {
                var shell = await GetServiceAsync(typeof(SVsShell)) as IVsShell;
                IVsPackage startPagePackage;
                if (ErrorHandler.Failed(shell?.LoadPackage(new Guid(StartPagePreview4PackageId), out startPagePackage) ?? -1))
                {
                    // ¯\_(ツ)_/¯
                }
            }
        }

        tasks.Task<object> CreateService(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            AssemblyResolver.InitializeAssemblyResolver();
            var ret = Services.ComponentModel.DefaultExportProvider.GetExportedValueOrDefault<IUIProvider>();
            return tasks.Task.FromResult((object)ret);
        }
    }
}
