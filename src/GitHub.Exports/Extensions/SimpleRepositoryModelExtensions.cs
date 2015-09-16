using GitHub.Models;
using System;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

namespace GitHub.Extensions
{
    public static class SimpleRepositoryModelExtensions
    {
        /// <summary>
        /// Create a SimpleRepositoryModel from a VS git repo object
        /// </summary>
        public static ISimpleRepositoryModel ToModel(this IGitRepositoryInfo repo)
        {
            if (repo == null)
                return null;
            var uri = repo.GetUriFromRepository();
            var name = uri?.NameWithOwner;
            return name != null ? new SimpleRepositoryModel(name, uri, repo.RepositoryPath) : null;
        }

        public static bool HasCommits(this ISimpleRepositoryModel repository)
        {
            var repo = GitHelpers.GetRepoFromPath(repository.LocalPath);
            return repo?.Commits.Any() ?? false;
        }

        public static bool MightContainSolution(this ISimpleRepositoryModel repository)
        {
            var dir = new DirectoryInfo(repository.LocalPath);
            return dir.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                      .Any(x => ((x.Attributes.HasFlag(FileAttributes.Directory) || x.Attributes.HasFlag(FileAttributes.Normal)) &&
                                !x.Name.StartsWith(".", StringComparison.Ordinal) && !x.Name.StartsWith("readme", StringComparison.OrdinalIgnoreCase)));
        }
    }
}
