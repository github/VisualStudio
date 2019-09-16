using System;
using System.Threading;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using GitHub.VisualStudio.Essentials;

namespace GitHub.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidGitHubEssentialsPkgString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideUIContextRule(PackageGuids.GitHubExtensionNotEnabledUIContextString,
        name: "GitHub extension not enabled",
        expression: "!GitHubPackageExists",
        termNames: new[] { "GitHubPackageExists" },
        termValues: new[]
        {
            @"ConfigSettingsStoreQuery:Packages\{c3d3dc68-c977-411f-b3e8-03b0dccf7dfc}\AllowsBackgroundLoad"
        })]
    public class GitHubEssentialsPackage : AsyncPackage
    {
        IComponentModel componentModel;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            componentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(componentModel);

            await JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await GetServiceAsync(typeof(IMenuCommandService), true) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(PackageGuids.guidGitHubEssentialsCmdSet, PackageIds.addConnectionCommand);
                var menuItem = new MenuCommand(OnConnectToGitHub, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        void OnConnectToGitHub(object source, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var loginService = componentModel.DefaultExportProvider.GetExportedValue<LoginService>();
            loginService.ShowLoginDialog();
        }
    }
}
