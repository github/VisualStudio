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

        protected Task<UriString> GenerateLink()
        protected Task<UriString> GenerateLink(bool blame = false)
        {
            var repo = ActiveRepo;
            var activeDocument = ServiceProvider.TryGetService<IActiveDocumentSnapshot>();
            if (activeDocument == null)
                return null;
            return repo.GenerateUrl(activeDocument.Name, activeDocument.StartLine, activeDocument.EndLine, blame);
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
