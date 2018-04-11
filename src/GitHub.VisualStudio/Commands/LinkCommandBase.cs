using System;
using System.IO;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Exports;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using Serilog;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Base class for commands that produce a link to GitHub.com or an Enterprise instance.
    /// </summary>
    public abstract class LinkCommandBase : VsCommand
    {
        static readonly ILogger log = LogManager.ForContext<LinkCommandBase>();
        readonly Lazy<ISimpleApiClientFactory> apiFactory;
        readonly Lazy<IUsageTracker> usageTracker;

        protected LinkCommandBase(
            Guid commandSet,
            int commandId,
            IGitHubServiceProvider serviceProvider)
            : base(commandSet, commandId)
        {
            ServiceProvider = serviceProvider;
            apiFactory = new Lazy<ISimpleApiClientFactory>(() => ServiceProvider.TryGetService<ISimpleApiClientFactory>());
            usageTracker = new Lazy<IUsageTracker>(() => serviceProvider.TryGetService<IUsageTracker>());
        }

        protected ILocalRepositoryModel ActiveRepo { get; private set; }
        protected ISimpleApiClientFactory ApiFactory => apiFactory.Value;
        protected IGitHubServiceProvider ServiceProvider { get; }
        protected IUsageTracker UsageTracker => usageTracker.Value;

        public abstract override Task Execute();

        protected ILocalRepositoryModel GetRepositoryByPath(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var repo = ServiceProvider.TryGetService<IGitService>().GetRepository(path);
                    return new LocalRepositoryModel(repo.Info.WorkingDirectory.TrimEnd('\\'));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading the repository from '{Path}'", path);
            }

            return null;
        }

        protected ILocalRepositoryModel GetActiveRepo()
        {
            var activeRepo = ServiceProvider.TryGetService<ITeamExplorerServiceHolder>()?.ActiveRepo;
            // activeRepo can be null at this point because it is set elsewhere as the result of async operation that may not have completed yet.
            if (activeRepo == null)
            {
                var path = ServiceProvider.TryGetService<IVSGitServices>()?.GetActiveRepoPath() ?? String.Empty;
                try
                {
                    activeRepo = !string.IsNullOrEmpty(path) ? new LocalRepositoryModel(path) : null;
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Error loading the repository from '{Path}'", path);
                }
            }
            return activeRepo;
        }

        void RefreshRepo()
        {
            ActiveRepo = ServiceProvider.TryGetService<ITeamExplorerServiceHolder>().ActiveRepo;

            if (ActiveRepo == null)
            {
                var vsGitServices = ServiceProvider.TryGetService<IVSGitServices>();
                string path = vsGitServices?.GetActiveRepoPath() ?? String.Empty;
                try
                {
                    ActiveRepo = !String.IsNullOrEmpty(path) ? new LocalRepositoryModel(path) : null;
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Error loading the repository from '{Path}'", path);
                }
            }
        }

        protected async Task<bool> IsGitHubRepo()
        {
            RefreshRepo();

            var uri = ActiveRepo?.CloneUrl;
            if (uri == null)
                return false;

            var simpleApiClient = await ApiFactory.Create(uri);

            var isdotcom = HostAddress.IsGitHubDotComUri(uri.ToRepositoryUrl());
            if (!isdotcom)
            {
                var repo = await simpleApiClient.GetRepository();
                var activeRepoFullName = ActiveRepo.Owner + '/' + ActiveRepo.Name;
                return (repo.FullName == activeRepoFullName || repo.Id == 0) && await simpleApiClient.IsEnterprise();
            }
            return isdotcom;
        }

        protected async Task<bool> IsCurrentFileInGitHubRepository()
        {
            if (!await IsGitHubRepo())
                return false;

            var activeDocument = ServiceProvider.TryGetService<IActiveDocumentSnapshot>();

            return activeDocument != null &&
                IsFileDescendantOfDirectory(activeDocument.Name, ActiveRepo.LocalPath);
        }

        protected Task<UriString> GenerateLink(LinkType linkType)
        {
            var activeDocument = ServiceProvider.TryGetService<IActiveDocumentSnapshot>();
            if (activeDocument == null)
                return null;

            var repo = GetRepositoryByPath(activeDocument.Name);

            return repo.GenerateUrl(linkType, activeDocument.Name, activeDocument.StartLine, activeDocument.EndLine);
        }

        protected override void QueryStatus()
        {
            var githubRepoCheckTask = IsCurrentFileInGitHubRepository();
            Visible = githubRepoCheckTask.Wait(250) ? githubRepoCheckTask.Result : false;
        }

        // Taken from http://stackoverflow.com/a/26012991/6448
        public static bool IsFileDescendantOfDirectory(string file, string directory)
        {
            var fileInfo = new FileInfo(file);
            var directoryInfo = new DirectoryInfo(directory);

            // https://connect.microsoft.com/VisualStudio/feedback/details/777308/inconsistent-behavior-of-fullname-when-provided-path-ends-with-a-backslash
            string path = directoryInfo.FullName.TrimEnd(Path.DirectorySeparatorChar);
            DirectoryInfo dir = fileInfo.Directory;
            while (dir != null)
            {
                if (dir.FullName.TrimEnd(Path.DirectorySeparatorChar).Equals(path, StringComparison.OrdinalIgnoreCase))
                    return true;
                dir = dir.Parent;
            }
            return false;
        }
    }
}
