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
using System.ComponentModel.Design;
using GitHub.ViewModels;
using GitHub.Api;

namespace GitHub.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", System.AssemblyVersionInformation.Version, IconResourceID = 400)]
    [Guid(GuidList.guidGitHubPkgString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // this is the Git service GUID, so we load whenever it loads
    [ProvideAutoLoad(Guids.GitSccProviderId)]
    [ProvideToolWindow(typeof(GitHubPane), Orientation = ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindSolutionExplorer)]
    [ProvideOptionPage(typeof(OptionsPage), "GitHub for Visual Studio", "General", 0, 0, supportsAutomation: true)]
    public class GitHubPackage : AsyncPackage
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        readonly IServiceProvider serviceProvider;

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

            await GetServiceAsync(typeof(IUsageTracker));

            InitializeMenus().Forget();
        }

        async Task InitializeMenus()
        {
            var menus = await GetServiceAsync(typeof(IMenuProvider)) as IMenuProvider;

            await ThreadingHelper.SwitchToMainThreadAsync();

            foreach (var menu in menus.Menus)
                serviceProvider.AddCommandHandler(menu.Guid, menu.CmdId, (s, e) => menu.Activate());

            foreach (var menu in menus.DynamicMenus)
                serviceProvider.AddCommandHandler(menu.Guid, menu.CmdId, menu.CanShow, () => menu.Activate());
        }

        async Task EnsurePackageLoaded(Guid packageGuid)
        {
            var shell = await GetServiceAsync(typeof(SVsShell)) as IVsShell;
            if (shell != null)
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

    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideService(typeof(ILoginManager), IsAsyncQueryable = true)]
    [ProvideService(typeof(IMenuProvider), IsAsyncQueryable = true)]
    [ProvideService(typeof(IGitHubServiceProvider), IsAsyncQueryable = true)]
    [ProvideService(typeof(IUsageTracker), IsAsyncQueryable = true)]
    [ProvideService(typeof(IUIProvider), IsAsyncQueryable = true)]
    [ProvideService(typeof(IGitHubToolWindowManager))]
    [Guid(ServiceProviderPackageId)]
    public sealed class ServiceProviderPackage : AsyncPackage, IServiceProviderPackage, IGitHubToolWindowManager
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
            AddService(typeof(IGitHubServiceProvider), CreateService, true);
            AddService(typeof(IUsageTracker), CreateService, true);
            AddService(typeof(ILoginManager), CreateService, true);
            AddService(typeof(IMenuProvider), CreateService, true);
            AddService(typeof(IUIProvider), CreateService, true);
            AddService(typeof(IGitHubToolWindowManager), CreateService, true);
            return Task.CompletedTask;
        }

        public IViewHost ShowHomePane()
        {
            var pane = ShowToolWindow(new Guid(GitHubPane.GitHubPaneGuid));
            if (pane == null)
                return null;
            var frame = pane.Frame as IVsWindowFrame;
            if (frame != null)
            {
                ErrorHandler.Failed(frame.Show());
            }
            return pane as IViewHost;
        }

        static ToolWindowPane ShowToolWindow(Guid windowGuid)
        {
            IVsWindowFrame frame;
            if (ErrorHandler.Failed(Services.UIShell.FindToolWindow((uint) __VSCREATETOOLWIN.CTW_fForceCreate,
                ref windowGuid, out frame)))
            {
                VsOutputLogger.WriteLine("Unable to find or create GitHubPane '" + UI.GitHubPane.GitHubPaneGuid + "'");
                return null;
            }
            if (ErrorHandler.Failed(frame.Show()))
            {
                VsOutputLogger.WriteLine("Unable to show GitHubPane '" + UI.GitHubPane.GitHubPaneGuid + "'");
                return null;
            }

            object docView = null;
            if (ErrorHandler.Failed(frame.GetProperty((int) __VSFPROPID.VSFPROPID_DocView, out docView)))
            {
                VsOutputLogger.WriteLine("Unable to grab instance of GitHubPane '" + UI.GitHubPane.GitHubPaneGuid + "'");
                return null;
            }
            return docView as GitHubPane;
        }

        async Task<object> CreateService(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            if (serviceType == null)
                return null;

            if (container != this)
                return null;

            if (serviceType == typeof(IGitHubServiceProvider))
            {
                //var sp = await GetServiceAsync(typeof(SVsServiceProvider)) as IServiceProvider;
                var result = new GitHubServiceProvider(this, this);
                await result.Initialize();
                return result;
            }
            else if (serviceType == typeof(ILoginManager))
            {
                var serviceProvider = await GetServiceAsync(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;
                var loginCache = serviceProvider.GetService<ILoginCache>();
                var twoFaHandler = serviceProvider.GetService<ITwoFactorChallengeHandler>();

                return new LoginManager(
                    loginCache,
                    twoFaHandler,
                    ApiClientConfiguration.ClientId,
                    ApiClientConfiguration.ClientSecret,
                    ApiClientConfiguration.AuthorizationNote,
                    ApiClientConfiguration.MachineFingerprint);
            }
            else if (serviceType == typeof(IMenuProvider))
            {
                var sp = await GetServiceAsync(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;
                return new MenuProvider(sp);
            }
            else if (serviceType == typeof(IUsageTracker))
            {
                var uiProvider = await GetServiceAsync(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;
                return new UsageTracker(uiProvider);
            }
            else if (serviceType == typeof(IUIProvider))
            {
                var sp = await GetServiceAsync(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;
                return new UIProvider(sp);
            }
            else if (serviceType == typeof(IGitHubToolWindowManager))
            {
                return this;
            }
            // go the mef route
            else
            {
                var sp = await GetServiceAsync(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider;
                return sp.TryGetService(serviceType);
            }
        }
    }
}
