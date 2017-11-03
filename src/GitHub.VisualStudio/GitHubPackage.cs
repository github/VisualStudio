using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.VisualStudio.Menus;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Octokit;
using Serilog;
using Task = System.Threading.Tasks.Task;
using EnvDTE;

namespace GitHub.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", System.AssemblyVersionInformation.Version, IconResourceID = 400)]
    [Guid(Guids.guidGitHubPkgString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // this is the Git service GUID, so we load whenever it loads
    [ProvideAutoLoad(Guids.GitSccProviderId)]
    [ProvideToolWindow(typeof(GitHubPane), Orientation = ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindSolutionExplorer)]
    [ProvideOptionPage(typeof(OptionsPage), "GitHub for Visual Studio", "General", 0, 0, supportsAutomation: true)]
    public class GitHubPackage : AsyncPackage
    {
        static readonly ILogger log = LogManager.ForContext<GitHubPackage>();

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
            var packageVersion = GetType().Assembly.GetName().Version;
            var hostVersionInfo = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo;
            log.Information("Initializing GitHub Extension v{PackageVersion} in {$FileDescription} ({$ProductVersion})",
                packageVersion, hostVersionInfo.FileDescription, hostVersionInfo.ProductVersion);

            await base.InitializeAsync(cancellationToken, progress);

            await GetServiceAsync(typeof(IUsageTracker));

            InitializeMenus().Forget();
        }

        async Task InitializeMenus()
        {
            var menus = await GetServiceAsync(typeof(IMenuProvider)) as IMenuProvider;
            if (menus == null)
            {
                // Ignore if null because Expression Blend doesn't support custom services or menu extensibility.
                return;
            }

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
        static readonly ILogger log = LogManager.ForContext<ServiceProviderPackage>();

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
            if (ErrorHandler.Failed(Services.UIShell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate,
                ref windowGuid, out frame)))
            {
                log.Error("Unable to find or create GitHubPane '{Guid}'", UI.GitHubPane.GitHubPaneGuid);
                return null;
            }
            if (ErrorHandler.Failed(frame.Show()))
            {
                log.Error("Unable to show GitHubPane '{Guid}'", UI.GitHubPane.GitHubPaneGuid);
                return null;
            }

            object docView = null;
            if (ErrorHandler.Failed(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView)))
            {
                log.Error("Unable to grab instance of GitHubPane '{Guid}'", UI.GitHubPane.GitHubPaneGuid);
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
