using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using GitHub.Exports;
using GitHub.Logging;
using GitHub.Commands;
using GitHub.Services.Vssdk.Commands;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Serilog;
using Task = System.Threading.Tasks.Task;
using Microsoft;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Guids.InlineReviewsPackageId)]
    [ProvideAutoLoad(Guids.GitContextPkgString, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public class InlineReviewsPackage : AsyncPackage
    {
        static readonly ILogger log = LogManager.ForContext<InlineReviewsPackage>();

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            var componentModel = (IComponentModel)(await GetServiceAsync(typeof(SComponentModel)));
            Assumes.Present(componentModel);

            var exports = componentModel.DefaultExportProvider;

            // Avoid delays when there is ongoing UI activity.
            // See: https://github.com/github/VisualStudio/issues/1537
            await JoinableTaskFactory.RunAsync(VsTaskRunContext.UIThreadNormalPriority, InitializeMenus);
        }

        async Task InitializeMenus()
        {
            if (!ExportForVisualStudioProcessAttribute.IsVisualStudioProcess())
            {
                log.Warning("Don't initialize menus for non-Visual Studio process");
                return;
            }

            var componentModel = (IComponentModel)(await GetServiceAsync(typeof(SComponentModel)));
            Assumes.Present(componentModel);

            var exports = componentModel.DefaultExportProvider;
            var commands = new IVsCommandBase[]
            {
                exports.GetExportedValue<INextInlineCommentCommand>(),
                exports.GetExportedValue<IPreviousInlineCommentCommand>(),
                exports.GetExportedValue<IToggleInlineCommentMarginCommand>()
            };

            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var menuService = (IMenuCommandService)(await GetServiceAsync(typeof(IMenuCommandService)));
            Assumes.Present(menuService);

            menuService.AddCommands(commands);
        }
    }
}
