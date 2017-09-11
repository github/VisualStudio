using GitHub.Exports;
using GitHub.Primitives;
using GitHub.Services;
using System;
using System.Threading.Tasks;
using System.IO;
using GitHub.Models;

namespace GitHub.VisualStudio.Menus
{
    public class LinkMenuBase: MenuBase
    {
        readonly Lazy<IUsageTracker> usageTracker;

        protected IUsageTracker UsageTracker => usageTracker.Value;

        public LinkMenuBase(IGitHubServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            usageTracker = new Lazy<IUsageTracker>(() => ServiceProvider.TryGetService<IUsageTracker>());
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

        public bool CanShow()
        {
            var githubRepoCheckTask = IsCurrentFileInGitHubRepository();
            return githubRepoCheckTask.Wait(250) ? githubRepoCheckTask.Result : false;
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
