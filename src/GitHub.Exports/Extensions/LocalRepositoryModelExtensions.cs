using System;
using System.Linq;
using System.IO;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.Extensions
{
    public static class LocalRepositoryModelExtensions
    {
        public static bool HasCommits(this LocalRepositoryModel repository)
        {
            using (var repo = GitService.GitServiceHelper.GetRepository(repository.LocalPath))
            {
                return repo?.Commits.Any() ?? false;
            }
        }

        public static bool MightContainSolution(this LocalRepositoryModel repository)
        {
            var dir = new DirectoryInfo(repository.LocalPath);
            return dir.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                      .Any(x => ((x.Attributes.HasFlag(FileAttributes.Directory) || x.Attributes.HasFlag(FileAttributes.Normal)) &&
                                !x.Name.StartsWith(".", StringComparison.Ordinal) && !x.Name.StartsWith("readme", StringComparison.OrdinalIgnoreCase)));
        }
    }
}
