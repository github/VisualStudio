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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using GitHub.VisualStudio.Menus;

namespace GitHub.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidGitHubPkgString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // this is the Git service GUID, so we load whenever it loads
    [ProvideAutoLoad("11B8E6D7-C08B-4385-B321-321078CDD1F8")]
    [ProvideToolWindow(typeof(GitHubPane), Orientation = ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindSolutionExplorer)]
    [ProvideOptionPage(typeof(OptionsPage), "GitHub for Visual Studio", "General", 0, 0, supportsAutomation: true)]
    public class GitHubPackage : AsyncPackage
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
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

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            await EnsurePackageLoaded(new Guid(ServiceProviderPackage.ServiceProviderPackageId));

            // Activate the usage tracker by forcing an instance to be created.
            GetServiceAsync(typeof(IUsageTracker)).Forget();

            InitializeMenus().Forget();
        }

        async Task InitializeMenus()
        {
            var menus = await GetServiceAsync(typeof(IMenuProvider)) as IMenuProvider;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (var menu in menus.Menus)
                serviceProvider.AddCommandHandler(menu.Guid, menu.CmdId, (s, e) => menu.Activate());

            foreach (var menu in menus.DynamicMenus)
                serviceProvider.AddCommandHandler(menu.Guid, menu.CmdId, menu.CanShow, () => menu.Activate());
        }

        async Task EnsurePackageLoaded(Guid packageGuid)
        {
            var shell = await GetServiceAsync(typeof(SVsShell)) as IVsShell;
            if (shell  != null)
            {
                IVsPackage vsPackage;
                ErrorHandler.ThrowOnFailure(shell.LoadPackage(ref packageGuid, out vsPackage));
            }
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

    //[NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    //[PackageRegistration(UseManagedResourcesOnly = true)]
    //[ProvideAutoLoad(MenuLoadingContextId)]
    //[ProvideUIContextRule(MenuLoadingContextId, 
    //    name: "GitHub context menus",
    //    expression: "FileOpen",
    //    termNames: new[] { "FileOpen" },
    //    termValues: new[] { "ActiveEditorContentType:CSharp" }
    //)]
    //[Guid(MenuRegistrationPackageId)]
    //public sealed class MenuRegistrationPackage : Package
    //{
    //    const string MenuRegistrationPackageId = "E37D3B17-2255-4144-9802-349530796693";
    //    const string MenuLoadingContextId = "F2CC8C27-AF24-4BA6-80BC-4819A0E8844F";

    //    protected override void Initialize()
    //    {
    //        base.Initialize();


    //    }
    //}

    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideService(typeof(IMenuProvider), IsAsyncQueryable = true)]
    [ProvideService(typeof(IUIProvider), IsAsyncQueryable = true)]
    [ProvideService(typeof(IUsageTracker), IsAsyncQueryable = true)]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [Guid(ServiceProviderPackageId)]
    public sealed class ServiceProviderPackage : AsyncPackage
    {
        public const string ServiceProviderPackageId = "D5CE1488-DEDE-426D-9E5B-BFCCFBE33E53";
        const string StartPagePreview4PackageId = "3b764d23-faf7-486f-94c7-b3accc44a70d";
        const string StartPagePreview5PackageId = "3b764d23-faf7-486f-94c7-b3accc44a70e";

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

        protected override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            AddService(typeof(IUIProvider), CreateService, true);
            AddService(typeof(IUsageTracker), CreateService, true);
            AddService(typeof(IMenuProvider), CreateService, true);
            return Task.FromResult<object>(null);
        }

        async Task<object> CreateService(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            if (serviceType == null)
                return null;

            if (serviceType == typeof(IUIProvider))
            {
                var result = new GitHubServiceProvider(this);
                await result.Initialize();
                return result;
            }
            else if (serviceType == typeof(IMenuProvider))
            {
                var sp = await GetServiceAsync(typeof(IUIProvider)) as IUIProvider;
                return new MenuProvider(sp);
            }
            else if (serviceType == typeof(IUsageTracker))
            {
                var uiProvider = await GetServiceAsync(typeof(IUIProvider)) as IUIProvider;
                return new UsageTracker(uiProvider);
            }
            // go the mef route
            else
            {
                var sp = await GetServiceAsync(typeof(IUIProvider)) as IUIProvider;
                return sp.TryGetService(serviceType);
            }
        }
    }
}
