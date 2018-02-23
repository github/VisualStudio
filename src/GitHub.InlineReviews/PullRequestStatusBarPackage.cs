using System;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Services;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews
{
    [Guid(Guids.PullRequestStatusPackageId)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(Guids.GitSccProviderId, PackageAutoLoadFlags.BackgroundLoad)]
    public class PullRequestStatusBarPackage : AsyncPackage
    {
        /// <summary>
        /// Initialize the PR status UI on Visual Studio's status bar.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var componentModel = (IComponentModel)await GetServiceAsync(typeof(SComponentModel));
            var exportProvider = componentModel.DefaultExportProvider;
            var pullRequestStatusManager = exportProvider.GetExportedValue<IPullRequestStatusBarManager>();
            await pullRequestStatusManager.InitializeAsync();
        }
    }
}
