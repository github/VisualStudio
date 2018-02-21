using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using GitHub.Helpers;
using GitHub.Commands;
using GitHub.Services.Vssdk.Commands;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Guids.InlineReviewsPackageId)]
    [ProvideAutoLoad(Guids.UIContext_Git, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public class InlineReviewsPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            var menuService = (IMenuCommandService)(await GetServiceAsync(typeof(IMenuCommandService)));
            var componentModel = (IComponentModel)(await GetServiceAsync(typeof(SComponentModel)));
            var exports = componentModel.DefaultExportProvider;

            await ThreadingHelper.SwitchToMainThreadAsync();
            menuService.AddCommands(
                exports.GetExportedValue<INextInlineCommentCommand>(),
                exports.GetExportedValue<IPreviousInlineCommentCommand>());
        }
    }
}
