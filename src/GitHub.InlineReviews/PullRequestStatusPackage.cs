using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Services;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.PullRequestStatusPackageId)]
    [ProvideAutoLoad(Guids.GitSccProviderId)]
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
