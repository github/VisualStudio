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
    // Allow assemblies in the UI directory to be resolved by their full or partial name.
    // This is required for .imagemanifest files, XAML and when using unsigned assemblies.
    // See: https://github.com/github/VisualStudio/pull/1236/
    [ProvideBindingPath(SubPath = "UI")]

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.StartPagePackageId)]
    [ProvideCodeContainerProvider("GitHub Container", Guids.StartPagePackageId, Images.ImageMonikerGuid, Images.Logo, "#110", "#111", typeof(GitHubContainerProvider))]
    public sealed class StartPagePackage : ExtensionPointPackage
    {
        protected override void Initialize()
        {
            base.Initialize();
            ServiceProvider = this;
        }

        internal static IServiceProvider ServiceProvider { get; private set; }
    }

    [Guid(Guids.CodeContainerProviderId)]
    public class GitHubContainerProvider : ICodeContainerProvider
    {
        readonly ICodeContainerProvider provider;

        public GitHubContainerProvider() : this(StartPagePackage.ServiceProvider)
        {
        }

        public GitHubContainerProvider(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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
