using System;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.InlineReviews.Services;
using Microsoft.VisualStudio.Shell;
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
            var usageTracker = (IUsageTracker)await GetServiceAsync(typeof(IUsageTracker));
            var serviceProvider = (IGitHubServiceProvider)await GetServiceAsync(typeof(IGitHubServiceProvider));

            // NOTE: ThreadingHelper.SwitchToMainThreadAsync() doesn't return until a solution is loaded. Using Dispatcher.Invoke instead.
            Application.Current.Dispatcher.Invoke(() =>
            {
                var gitExt = serviceProvider.GetService<IVSGitExt>();
                new PullRequestStatusBarManager(gitExt, usageTracker, serviceProvider);
            });
        }
    }
}
