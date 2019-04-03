using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;
using CodeContainer = Microsoft.VisualStudio.Shell.CodeContainerManagement.CodeContainer;
using ICodeContainerProvider = Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerProvider;

namespace GitHub.StartPage
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.StartPagePackageId)]
    [ProvideCodeContainerProvider("GitHub Container", Guids.StartPagePackageId, Images.ImageMonikerGuid, Images.Logo, "#110", "#111", typeof(GitHubContainerProvider))]
    public sealed class StartPagePackage : ExtensionPointPackage
    {
    }

    [Guid(Guids.CodeContainerProviderId)]
    public class GitHubContainerProvider : ICodeContainerProvider
    {
        readonly ICodeContainerProvider provider;

        public GitHubContainerProvider() : this(null)
        {
        }

        public GitHubContainerProvider(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            serviceProvider = serviceProvider ?? ServiceProvider.GlobalProvider;
            var factory = new ExtensionServicesFactory(serviceProvider);
            var services = factory.Create();
            provider = services.GetGitHubContainerProvider();
        }

        public Task<CodeContainer> AcquireCodeContainerAsync(IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {
            return provider.AcquireCodeContainerAsync(downloadProgress, cancellationToken);
        }

        public Task<CodeContainer> AcquireCodeContainerAsync(RemoteCodeContainer onlineCodeContainer, IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {
            return provider.AcquireCodeContainerAsync(onlineCodeContainer, downloadProgress, cancellationToken);
        }
    }
}
