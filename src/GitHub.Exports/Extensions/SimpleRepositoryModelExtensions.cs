using System;
using System.Globalization;
using System.Linq;
using System.IO;
using GitHub.Models;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using GitHub.Services;
using GitHub.VisualStudio;
using GitHub.Primitives;
using System.Text;
using System.Diagnostics;

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
            var repo = GitService.GitServiceHelper.GetRepo(repository.LocalPath);
            return repo.Commits.FirstOrDefault()?.Sha;
        }

        public static Uri GenerateUrl(this ISimpleRepositoryModel repository, string path = null,
            int startLine = -1, int endLine = -1)
        {
            var cloneUrl = repository.CloneUrl;
            if (cloneUrl == null)
                return null;

            if (path != null && Path.IsPathRooted(path))
            {
                // if the path root doesn't match the repository local path, then ignore it
                if (repository.LocalPath == null ||
                    !path.StartsWith(repository.LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Assert(false, String.Format(CultureInfo.CurrentCulture, "GenerateUrl: path {0} doesn't match repository {1}", path, repository.LocalPath));
                    path = null;
                }
                else
                    path = path.Substring(repository.LocalPath.Length + 1);
            }

            var commitSha = repository.CurrentSha();
            var uri = GenerateUrl(cloneUrl.ToRepositoryUrl().AbsoluteUri, commitSha, path, startLine, endLine);
            return new UriString(uri).ToUri();
        }

        static string GenerateUrl(string basePath, string sha, string path, int startLine = -1, int endLine = -1)
        {
            var sb = new StringBuilder(basePath);
            if (sha == null)
                return sb.ToString();

            if (String.IsNullOrEmpty(path))
            {
                sb.AppendFormat("/commit/{0}", sha);
                return sb.ToString();
            }

            sb.AppendFormat("/blob/{0}/{1}", sha, path.Replace(@"\", "/"));
            if (startLine < 0)
                return sb.ToString();
            sb.AppendFormat("#L{0}", startLine);
            if (endLine < 0)
                return sb.ToString();
            sb.AppendFormat("-L{0}", endLine);
            return sb.ToString();
        }
    }
}
