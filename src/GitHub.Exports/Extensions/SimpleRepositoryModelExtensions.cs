using System;
using System.Globalization;
using System.Linq;
using System.IO;
using GitHub.Models;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using GitHub.Services;
using GitHub.VisualStudio;

namespace GitHub.Extensions
{
    public static class SimpleRepositoryModelExtensions
    {
        /// <summary>
        /// Create a SimpleRepositoryModel from a VS git repo object
        /// </summary>
        public static ISimpleRepositoryModel ToModel(this IGitRepositoryInfo repo)
        {
            return repo == null ? null : new SimpleRepositoryModel(repo.RepositoryPath);
        }

        public static bool HasCommits(this ISimpleRepositoryModel repository)
        {
            var repo = GitService.GitServiceHelper.GetRepo(repository.LocalPath);
            return repo?.Commits.Any() ?? false;
        }

        public static bool MightContainSolution(this ISimpleRepositoryModel repository)
        {
            var dir = new DirectoryInfo(repository.LocalPath);
            return dir.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                      .Any(x => ((x.Attributes.HasFlag(FileAttributes.Directory) || x.Attributes.HasFlag(FileAttributes.Normal)) &&
                                !x.Name.StartsWith(".", StringComparison.Ordinal) && !x.Name.StartsWith("readme", StringComparison.OrdinalIgnoreCase)));
        }

        public static string CurrentSha(this ISimpleRepositoryModel repository)
        {
            if (repository == null)
                return null;

            var repo = GitService.GitServiceHelper.GetRepo(repository.LocalPath);

            if (repo == null)
                return null;

            return !repo.Commits.Any() ? null : repo.Commits.ElementAt(0).Sha;
        }

        public static string BrowserUrl(this ISimpleRepositoryModel repository, IActiveDocument activeDocument)
        {
            if (repository == null || activeDocument == null)
                return null;

            var currentCommitSha = repository.CurrentSha();
            var currentCloneUrl = repository.CloneUrl;
            var localPath = repository.LocalPath;
            var lineTag = "L" + activeDocument.AnchorLine;

            if (string.IsNullOrEmpty(currentCommitSha) || string.IsNullOrEmpty(currentCloneUrl) ||
                string.IsNullOrEmpty(localPath))
                return null;

            if (activeDocument.AnchorLine != activeDocument.EndLine)
            {
                lineTag += "-L" + activeDocument.EndLine;
            }

            var outputUri = string.Format(CultureInfo.CurrentCulture, "{0}/blob/{1}{2}#{3}",
                currentCloneUrl,
                currentCommitSha,
                activeDocument.Name.Replace(localPath, "").Replace("\\", "/"),
                lineTag);

            return outputUri;
        }
    }
}
