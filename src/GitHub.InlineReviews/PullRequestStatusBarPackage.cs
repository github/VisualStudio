using System;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Services;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.PullRequestStatusPackageId)]
    [ProvideAutoLoad(Guids.GitSccProviderId)]
    public class PullRequestStatusBarPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var componentModel = (IComponentModel)await GetServiceAsync(typeof(SComponentModel));
            var exportProvider = componentModel.DefaultExportProvider;
            var pullRequestStatusManager = exportProvider.GetExportedValue<IPullRequestStatusBarManager>();
            pullRequestStatusManager.Initialize(Application.Current.MainWindow);
        }
    }
}
