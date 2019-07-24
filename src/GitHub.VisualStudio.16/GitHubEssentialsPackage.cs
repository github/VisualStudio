using System;
using System.Threading;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using EnvDTE;
using GitHub.Services;
using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

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

            if (FullExtensionUtilities.IsInstalled(this))
            {
                if (GetService(typeof(DTE)) is DTE dte &&
                    dte.Commands is var commands &&
                    commands.Item(Guids.guidGitHubCmdSet, PkgCmdIDList.addConnectionCommand) is Command command &&
                    command.IsAvailable)
                {
                    commands.Raise(command.Guid, command.ID, null, null);
                }
            }
            else
            {
                var compositionServices = componentModel.DefaultExportProvider.GetExportedValue<CompositionServices>();
                var exportProvider = compositionServices.GetExportProvider();
                var dialogService = exportProvider.GetExportedValue<IDialogService>();
                JoinableTaskFactory.Run(() => dialogService.ShowLoginDialog());
            }
        }
    }
}
