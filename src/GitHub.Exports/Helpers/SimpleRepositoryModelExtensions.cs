using GitHub.Models;
using System;
using System.Linq;
using System.IO;

namespace GitHub.Extensions
{
    using VisualStudio;

    public static class SimpleRepositoryModelExtensions
    {
        public static bool HasCommits(this ISimpleRepositoryModel repository)
        {
            var repo = Services.GetRepoFromPath(repository.LocalPath);
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
