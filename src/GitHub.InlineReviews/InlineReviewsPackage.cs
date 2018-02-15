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
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Guids.InlineReviewsPackageId)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(PullRequestCommentsPane), DocumentLikeTool=true)]
    public class InlineReviewsPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var menuService = (IMenuCommandService)(await GetServiceAsync(typeof(IMenuCommandService)));
            var componentModel = (IComponentModel)(await GetServiceAsync(typeof(SComponentModel)));
            var exports = componentModel.DefaultExportProvider;

            menuService.AddCommand(exports.GetExportedValue<INextInlineCommentCommand>());
            menuService.AddCommand(exports.GetExportedValue<IPreviousInlineCommentCommand>());
        }
    }
}
