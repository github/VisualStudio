using System;
using System.Linq;
using System.IO;
using GitHub.Models;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using GitHub.Services;

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
    }
}
