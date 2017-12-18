using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Services;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.PullRequestStatusPackageId)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public class PullRequestStatusPackage : Package
    {
        protected override void Initialize()
        {
            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            var exportProvider = componentModel.DefaultExportProvider;
            var pullRequestStatusManager = exportProvider.GetExportedValue<IPullRequestStatusManager>();
            pullRequestStatusManager.Initialize();
        }
    }
}
