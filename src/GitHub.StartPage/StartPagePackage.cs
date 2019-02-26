using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;
using Microsoft.VisualStudio.Threading;
using Serilog;
using CodeContainer = Microsoft.VisualStudio.Shell.CodeContainerManagement.CodeContainer;
using ICodeContainerProvider = Microsoft.VisualStudio.Shell.CodeContainerManagement.ICodeContainerProvider;
using Task = System.Threading.Tasks.Task;

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
        static readonly ILogger log = LogManager.ForContext<GitHubContainerProvider>();

        readonly Lazy<IGitHubServiceProvider> gitHubServiceProvider;

        public GitHubContainerProvider() : this(
            new Lazy<IGitHubServiceProvider>(() => Package.GetGlobalService(typeof(IGitHubServiceProvider)) as IGitHubServiceProvider))
        {
        }

        public GitHubContainerProvider(Lazy<IGitHubServiceProvider> gitHubServiceProvider)
        {
            this.gitHubServiceProvider = gitHubServiceProvider;
        }

        public async Task<CodeContainer> AcquireCodeContainerAsync(IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {
            return await RunAcquisition(downloadProgress, null, cancellationToken);
        }

        public async Task<CodeContainer> AcquireCodeContainerAsync(RemoteCodeContainer onlineCodeContainer, IProgress<ServiceProgressData> downloadProgress, CancellationToken cancellationToken)
        {
            var repository = new RepositoryModel(onlineCodeContainer.Name, UriString.ToUriString(onlineCodeContainer.DisplayUrl));
            return await RunAcquisition(downloadProgress, repository, cancellationToken);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
        async Task<CodeContainer> RunAcquisition(IProgress<ServiceProgressData> downloadProgress, RepositoryModel repository, CancellationToken cancellationToken)
        {
            CloneDialogResult request = null;

            try
            {
                var uiProvider = await Task.Run(() => gitHubServiceProvider.Value);
                request = await ShowCloneDialog(uiProvider, downloadProgress, cancellationToken, repository);
            }
            catch (Exception e)
            {
                log.Error(e, "Error showing Start Page clone dialog");
            }

            if (request == null)
                return null;

            var uri = request.Url.ToRepositoryUrl();
            var repositoryName = request.Url.RepositoryName;

            // Report all steps complete before returning a CodeContainer
            downloadProgress.Report(new ServiceProgressData(string.Empty, string.Empty, 1, 1));

            return new CodeContainer(
                localProperties: new CodeContainerLocalProperties(request.Path, CodeContainerType.Folder,
                                new CodeContainerSourceControlProperties(repositoryName, request.Path, new Guid(Guids.GitSccProviderId))),
                remote: new RemoteCodeContainer(repositoryName,
                                                new Guid(Guids.CodeContainerProviderId),
                                                uri,
                                                new Uri(uri.ToString().TrimSuffix(".git")),
                                                DateTimeOffset.UtcNow),
                isFavorite: false,
                lastAccessed: DateTimeOffset.UtcNow);
        }

        static async Task<CloneDialogResult> ShowCloneDialog(
            IGitHubServiceProvider gitHubServiceProvider,
            IProgress<ServiceProgressData> progress,
            CancellationToken cancellationToken,
            RepositoryModel repository = null)
        {
            var dialogService = gitHubServiceProvider.GetService<IDialogService>();
            var cloneService = gitHubServiceProvider.GetService<IRepositoryCloneService>();
            var usageTracker = gitHubServiceProvider.GetService<IUsageTracker>();

            var cloneUrl = repository?.CloneUrl;
            if (await dialogService.ShowCloneDialog(null, cloneUrl) is CloneDialogResult result)
            {
                try
                {
                    await cloneService.CloneOrOpenRepository(result, progress, cancellationToken);
                    usageTracker.IncrementCounter(x => x.NumberOfStartPageClones).Forget();
                    return result;
                }
                catch
                {
                    var teServices = gitHubServiceProvider.TryGetService<ITeamExplorerServices>();
                    teServices.ShowError($"Failed to clone the repository '{result.Url.RepositoryName}'");
                }
            }

            return null;
        }
    }
}
