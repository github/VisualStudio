using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using System.IO;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;
using ICodeContainerProvider = Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerProvider;
using CodeContainer = Microsoft.VisualStudio.Shell.CodeContainerManagement.CodeContainer;
using Task = System.Threading.Tasks.Task;
using System.ComponentModel;
using GitHub.Models;
using GitHub.Extensions;
using GitHub.Primitives;
using GitHub.VisualStudio;

namespace GitHub.StartPage
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.StartPagePackageId)]
    [ProvideCodeContainerProvider("GitHub Container", Guids.StartPagePackageId, Guids.ImagesId, 1, "#110", "#111", typeof(GitHubContainerProvider))]
    public sealed class StartPagePackage : ExtensionPointPackage
    {
        static IServiceProvider serviceProvider;
        internal static IServiceProvider ServiceProvider { get { return serviceProvider; } }

        public StartPagePackage()
        {
            serviceProvider = this;
        }
    }

    [Guid(Guids.CodeContainerProviderId)]
    public class GitHubContainerProvider : ICodeContainerProvider
    {
        public async Task<CodeContainer> AcquireCodeContainerAsync(IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {

            return await RunAcquisition(downloadProgress, cancellationToken, null);
        }

        public async Task<CodeContainer> AcquireCodeContainerAsync(RemoteCodeContainer onlineCodeContainer, IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {
            var repository = new RepositoryModel(onlineCodeContainer.Name, UriString.ToUriString(onlineCodeContainer.DisplayUrl));
            return await RunAcquisition(downloadProgress, cancellationToken, repository);
        }

        async Task<CodeContainer> RunAcquisition(IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken, IRepositoryModel repository)
        {
            CloneDialogResult request = null;

            try
            {
                var uiProvider = await Task.Run(() => Package.GetGlobalService(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider);
                await ShowTeamExplorerPage(uiProvider);
                request = await ShowCloneDialog(uiProvider, downloadProgress, repository);
            }
            catch
            {
                // TODO: log
            }

            if (request == null)
                return null;

            var path = Path.Combine(request.BasePath, request.Repository.Name);
            var uri = request.Repository.CloneUrl.ToRepositoryUrl();
            return new CodeContainer(
                localProperties: new CodeContainerLocalProperties(path, CodeContainerType.Folder,
                                new CodeContainerSourceControlProperties(request.Repository.Name, path, new Guid(Guids.GitSccProviderId))),
                remote: new RemoteCodeContainer(request.Repository.Name,
                                                new Guid(Guids.CodeContainerProviderId),
                                                uri,
                                                new Uri(uri.ToString().TrimSuffix(".git")),
                                                DateTimeOffset.UtcNow),
                isFavorite: false,
                lastAccessed: DateTimeOffset.UtcNow);
        }

        async Task ShowTeamExplorerPage(IGitHubServiceProvider gitHubServiceProvider)
        {
            var te = gitHubServiceProvider?.GetService(typeof(ITeamExplorer)) as ITeamExplorer;

            if (te != null)
            {
                var page = te.NavigateToPage(new Guid(TeamExplorerPageIds.Connect), null);

                if (page == null)
                {
                    var tcs = new TaskCompletionSource<ITeamExplorerPage>();
                    PropertyChangedEventHandler handler = null;

                    handler = new PropertyChangedEventHandler((s, e) =>
                    {
                        if (e.PropertyName == "CurrentPage")
                        {
                            tcs.SetResult(te.CurrentPage);
                            te.PropertyChanged -= handler;
                        }
                    });

                    te.PropertyChanged += handler;

                    page = await tcs.Task;
                }
            }
        }

        async Task<CloneDialogResult> ShowCloneDialog(
            IGitHubServiceProvider gitHubServiceProvider,
            IProgress<ServiceProgressData> progress,
            IRepositoryModel repository = null)
        {
            var dialogService = gitHubServiceProvider.GetService<IDialogService>();
            var cloneService = gitHubServiceProvider.GetService<IRepositoryCloneService>();
            CloneDialogResult result = null;
            
            if (repository == null)
            {
                result = await dialogService.ShowCloneDialog(null);
            }
            else
            {
                var basePath = await dialogService.ShowReCloneDialog(repository);

                if (basePath != null)
                {
                    result = new CloneDialogResult(basePath, repository);
                }
            }
            
            if (result != null)
            {
                try
                {
                    await cloneService.CloneRepository(
                        result.Repository.CloneUrl,
                        result.Repository.Name,
                        result.BasePath,
                        progress);
                }
                catch
                {
                    var teServices = gitHubServiceProvider.TryGetService<ITeamExplorerServices>();
                    teServices.ShowError($"Failed to clone the repository '{result.Repository.Name}'");
                    result = null;
                }
            }

            return result;
        }
    }
}
