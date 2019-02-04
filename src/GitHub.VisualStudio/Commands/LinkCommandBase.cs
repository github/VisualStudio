using System;
using System.Globalization;
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
        readonly Lazy<IGitService> lazyGitService;

        protected LinkCommandBase(
            Guid commandSet,
            int commandId,
            IGitHubServiceProvider serviceProvider,
            Lazy<IGitService> gitService)
            : base(commandSet, commandId)
        {
            ServiceProvider = serviceProvider;
            apiFactory = new Lazy<ISimpleApiClientFactory>(() => ServiceProvider.TryGetService<ISimpleApiClientFactory>());
            usageTracker = new Lazy<IUsageTracker>(() => serviceProvider.TryGetService<IUsageTracker>());
            lazyGitService = gitService;
        }

        protected LocalRepositoryModel ActiveRepo { get; private set; }
        protected ISimpleApiClientFactory ApiFactory => apiFactory.Value;
        protected IGitHubServiceProvider ServiceProvider { get; }
        protected IUsageTracker UsageTracker => usageTracker.Value;

        public abstract override Task Execute();

        protected LocalRepositoryModel GetRepositoryByPath(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var repo = ServiceProvider.TryGetService<IGitService>().GetRepository(path);
                    return GitService.GitServiceHelper.CreateLocalRepositoryModel(repo.Info.WorkingDirectory.TrimEnd('\\'));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error loading the repository from '{Path}'", path);
            }

            return null;
        }

        protected LocalRepositoryModel GetActiveRepo()
        {
            var activeRepo = ServiceProvider.TryGetService<ITeamExplorerServiceHolder>()?.TeamExplorerContext.ActiveRepository;
            // activeRepo can be null at this point because it is set elsewhere as the result of async operation that may not have completed yet.
            if (activeRepo == null)
            {
                var path = ServiceProvider.TryGetService<IVSGitServices>()?.GetActiveRepoPath() ?? String.Empty;
                try
                {
                    activeRepo = !string.IsNullOrEmpty(path) ? GitService.GitServiceHelper.CreateLocalRepositoryModel(path) : null;
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
            ActiveRepo = ServiceProvider.TryGetService<ITeamExplorerServiceHolder>()?.TeamExplorerContext.ActiveRepository;

            if (ActiveRepo == null)
            {
                var vsGitServices = ServiceProvider.TryGetService<IVSGitServices>();
                string path = vsGitServices?.GetActiveRepoPath() ?? String.Empty;
                try
                {
                    ActiveRepo = !String.IsNullOrEmpty(path) ? GitService.GitServiceHelper.CreateLocalRepositoryModel(path) : null;
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

            return GenerateUrl(lazyGitService.Value, repo, linkType, activeDocument.Name, activeDocument.StartLine, activeDocument.EndLine);
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

        /// <summary>
        /// Generates a http(s) url to the repository in the remote server, optionally
        /// pointing to a specific file and specific line range in it.
        /// </summary>
        /// <param name="linkType">Type of link to generate</param>
        /// <param name="path">The file to generate an url to. Optional.</param>
        /// <param name="startLine">A specific line, or (if specifying the <paramref name="endLine"/> as well) the start of a range</param>
        /// <param name="endLine">The end of a line range on the specified file.</param>
        /// <returns>An UriString with the generated url, or null if the repository has no remote server configured or if it can't be found locally</returns>
        public static async Task<UriString> GenerateUrl(IGitService gitService,
            LocalRepositoryModel repo, LinkType linkType, string path = null, int startLine = -1, int endLine = -1)
        {
            if (repo.CloneUrl == null)
                return null;

            var sha = await gitService.GetLatestPushedSha(path ?? repo.LocalPath);
            // this also incidentally checks whether the repo has a valid LocalPath
            if (String.IsNullOrEmpty(sha))
                return repo.CloneUrl.ToRepositoryUrl().AbsoluteUri;

            if (path != null && Path.IsPathRooted(path))
            {
                // if the path root doesn't match the repository local path, then ignore it
                if (!path.StartsWith(repo.LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.Assert(false, String.Format(CultureInfo.CurrentCulture, "GenerateUrl: path {0} doesn't match repository {1}", path, repo.LocalPath));
                    path = null;
                }
                else
                    path = path.Substring(repo.LocalPath.Length + 1);
            }

            if (startLine > 0 && endLine > 0 && startLine > endLine)
            {
                // if startLine is greater than endLine and both are set, swap them
                var temp = startLine;
                startLine = endLine;
                endLine = temp;
            }

            if (startLine == endLine)
            {
                // if startLine is the same as endLine don't generate a range link
                endLine = -1;
            }

            return new UriString(GenerateUrl(linkType, repo.CloneUrl.ToRepositoryUrl().AbsoluteUri, sha, path, startLine, endLine));
        }

        const string CommitFormat = "{0}/commit/{1}";
        const string BlobFormat = "{0}/blob/{1}/{2}";
        const string BlameFormat = "{0}/blame/{1}/{2}";
        const string StartLineFormat = "{0}#L{1}";
        const string EndLineFormat = "{0}-L{1}";
        static string GenerateUrl(LinkType linkType, string basePath, string sha, string path, int startLine = -1, int endLine = -1)
        {
            if (sha == null)
                return basePath;

            if (String.IsNullOrEmpty(path))
                return String.Format(CultureInfo.InvariantCulture, CommitFormat, basePath, sha);

            var ret = String.Format(CultureInfo.InvariantCulture, GetLinkFormat(linkType), basePath, sha, path.Replace(@"\", "/"));

            if (startLine < 0)
                return ret;
            ret = String.Format(CultureInfo.InvariantCulture, StartLineFormat, ret, startLine);
            if (endLine < 0)
                return ret;
            return String.Format(CultureInfo.InvariantCulture, EndLineFormat, ret, endLine);
        }

        /// <summary>
        /// Selects the proper format for the link type, defaults to the blob url when link type is not selected.
        /// </summary>
        /// <param name="linkType">Type of link to generate</param>
        /// <returns>The string format of the selected link type</returns>
        static string GetLinkFormat(LinkType linkType)
        {
            switch (linkType)
            {
                case LinkType.Blame:
                    return BlameFormat;

                case LinkType.Blob:
                    return BlobFormat;

                default:
                    return BlobFormat;
            }
        }
    }
}
