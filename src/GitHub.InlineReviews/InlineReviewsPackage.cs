using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using GitHub.Commands;
using GitHub.InlineReviews.Views;
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
    [ProvideToolWindow(typeof(PullRequestCommentsPane), DocumentLikeTool = true)]
    public class InlineReviewsPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            var menuService = (IMenuCommandService)(await GetServiceAsync(typeof(IMenuCommandService)));
            var componentModel = (IComponentModel)(await GetServiceAsync(typeof(SComponentModel)));
            var exports = componentModel.DefaultExportProvider;

            menuService.AddCommands(
                exports.GetExportedValue<INextInlineCommentCommand>(),
                exports.GetExportedValue<IPreviousInlineCommentCommand>());
        }

        // The IDesignerHost and ISelectionService services are requested by MenuCommandService.EnsureVerbs().
        // When called from a non-Main thread this would throw despite the fact these services don't exist.
        // This override allows IMenuCommandService.AddCommands to be called form a background thread.
        protected override object GetService(Type serviceType)
            => (serviceType == typeof(ISelectionService) || serviceType == typeof(IDesignerHost)) ? null : base.GetService(serviceType);
    }
}
