using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHubCore;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;
using Microsoft.VisualStudio.ComponentModelHost;
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
        public async Task<CodeContainer> AcquireCodeContainerAsync(IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {
            return await RunAcquisitionAsync(downloadProgress, cancellationToken, null);
        }

        public async Task<CodeContainer> AcquireCodeContainerAsync(RemoteCodeContainer onlineCodeContainer, IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {
            var url = onlineCodeContainer.DisplayUrl.ToString();
            return await RunAcquisitionAsync(downloadProgress, cancellationToken, url);
        }

        async Task<CodeContainer> RunAcquisitionAsync(IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken, string url = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var componentModel = await ServiceProvider.GetGlobalServiceAsync<SComponentModel, IComponentModel>();
            Assumes.Present(componentModel);

            var result = await ShowCloneDialogAsync(componentModel, downloadProgress, cancellationToken, url);
            if (result == null)
            {
                return null;
            }

            var repositoryName = result.Url.RepositoryName;
            var repositoryRootFullPath = result.Path;
            var sccProvider = new Guid(Guids.GitSccProviderId);
            var codeContainerProvider = new Guid(Guids.CodeContainerProviderId);
            var displayUrl = result.Url.ToRepositoryUrl();
            var browseOnlineUrl = new Uri(displayUrl.ToString().TrimSuffix(".git"));
            var lastAccessed = DateTimeOffset.UtcNow;

            var codeContainer = new CodeContainer(
                localProperties: new CodeContainerLocalProperties(repositoryRootFullPath, CodeContainerType.Folder,
                    new CodeContainerSourceControlProperties(repositoryName, repositoryRootFullPath, sccProvider)),
                remote: new RemoteCodeContainer(repositoryName, codeContainerProvider, displayUrl, browseOnlineUrl, lastAccessed),
                isFavorite: false,
                lastAccessed: lastAccessed);

            // Report all steps complete before returning a CodeContainer
            downloadProgress.Report(new ServiceProgressData(string.Empty, string.Empty, 1, 1));

            return codeContainer;
        }

        static async Task<CloneDialogResult> ShowCloneDialogAsync(IComponentModel componentModel,
            IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken, string url = null)
        {
            var compositionServices = new CompositionServices();
            var compositionContainer = compositionServices.CreateCompositionContainer(componentModel.DefaultExportProvider);

            var dialogService = compositionContainer.GetExportedValue<IDialogService>();
            var cloneDialogResult = await dialogService.ShowCloneDialog(null, url);
            if (cloneDialogResult != null)
            {
                var repositoryCloneService = compositionContainer.GetExportedValue<IRepositoryCloneService>();
                await repositoryCloneService.CloneOrOpenRepository(cloneDialogResult, downloadProgress, cancellationToken);
                return cloneDialogResult;
            }

            return null;
        }
    }
}
