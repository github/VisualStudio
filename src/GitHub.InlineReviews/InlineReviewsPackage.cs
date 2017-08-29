using System;
using System.Runtime.InteropServices;
using GitHub.InlineReviews.Commands;
using GitHub.InlineReviews.Views;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using GitHub.InlineReviews.Services;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.InlineReviewsPackageId)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(PullRequestCommentsPane), DocumentLikeTool = true)]
    public class InlineReviewsPackage : Package
    {
        protected override void Initialize()
        {
            base.Initialize();
            PackageResources.Register(this);
            ShowPullRequestStatus();
        }

        void ShowPullRequestStatus()
        {
            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            var exportProvider = componentModel.DefaultExportProvider;
            var pullRequestStatusManager = exportProvider.GetExportedValue<IPullRequestStatusManager>();
            pullRequestStatusManager.ShowStatus();
        }
    }
}
