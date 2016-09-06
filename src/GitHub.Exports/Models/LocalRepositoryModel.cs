using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using GitHub.Primitives;
using GitHub.UI;
using GitHub.Services;

namespace GitHub.Models
{
    /// <summary>
    /// A local repository.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LocalRepositoryModel : RepositoryModelBase, ILocalRepositoryModel, IEquatable<LocalRepositoryModel>
    {
        public LocalRepositoryModel(string name, UriString cloneUrl, string localPath = null)
            : base(name, cloneUrl)
        {
            LocalPath = localPath;
            Icon = Octicon.repo;
        }

        public LocalRepositoryModel(string path)
            : base(ExtractRepositoryName(path), ExtractCloneUrl(path))
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new ArgumentException("Path does not exist", nameof(path));
            LocalPath = path;
            Icon = Octicon.repo;
        }

        public void Refresh()
        {
            if (LocalPath == null)
                return;
            CloneUrl = GitService.GitServiceHelper.GetUri(LocalPath);
        }

        /// <summary>
        /// Generates a http(s) url to the repository in the remote server, optionally
        /// pointing to a specific file and specific line range in it.
        /// </summary>
        /// <param name="path">The file to generate an url to. Optional.</param>
        /// <param name="startLine">A specific line, or (if specifying the <paramref name="endLine"/> as well) the start of a range</param>
        /// <param name="endLine">The end of a line range on the specified file.</param>
        /// <returns>An UriString with the generated url, or null if the repository has no remote server configured or if it can't be found locally</returns>
        public UriString GenerateUrl(string path = null, int startLine = -1, int endLine = -1)
        {
            if (CloneUrl == null)
                return null;

            var sha = HeadSha;
            // this also incidentally checks whether the repo has a valid LocalPath
            if (String.IsNullOrEmpty(sha))
                return CloneUrl.ToRepositoryUrl().AbsoluteUri;

            if (path != null && Path.IsPathRooted(path))
            {
                // if the path root doesn't match the repository local path, then ignore it
                if (!path.StartsWith(LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Assert(false, String.Format(CultureInfo.CurrentCulture, "GenerateUrl: path {0} doesn't match repository {1}", path, LocalPath));
                    path = null;
                }
                else
                    path = path.Substring(LocalPath.Length + 1);
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

            return new UriString(GenerateUrl(CloneUrl.ToRepositoryUrl().AbsoluteUri, sha, path, startLine, endLine));
        }

        const string CommitFormat = "{0}/commit/{1}";
        const string BlobFormat = "{0}/blob/{1}/{2}";
        const string StartLineFormat = "{0}#L{1}";
        const string EndLineFormat = "{0}-L{1}";
        static string GenerateUrl(string basePath, string sha, string path, int startLine = -1, int endLine = -1)
        {
            if (sha == null)
                return basePath;

            if (String.IsNullOrEmpty(path))
                return String.Format(CultureInfo.InvariantCulture, CommitFormat, basePath, sha);

            var ret = String.Format(CultureInfo.InvariantCulture, BlobFormat, basePath, sha, path.Replace(@"\", "/"));
            if (startLine < 0)
                return ret;
            ret = String.Format(CultureInfo.InvariantCulture, StartLineFormat, ret, startLine);
            if (endLine < 0)
                return ret;
            return String.Format(CultureInfo.InvariantCulture, EndLineFormat, ret, endLine);
        }

        public string LocalPath { get; }

        public string HeadSha
        {
            get
            {
                var repo = GitService.GitServiceHelper.GetRepository(LocalPath);
                return repo?.Commits.FirstOrDefault()?.Sha ?? String.Empty;
            }
        }

        public IBranch CurrentBranch
        {
            get
            {
                var repo = GitService.GitServiceHelper.GetRepository(LocalPath);
                return new BranchModel(repo?.Head, this);
            }
        }


        /// <summary>
        /// Note: We don't consider CloneUrl a part of the hash code because it can change during the lifetime
        /// of a repository. Equals takes care of any hash collisions because of this
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 17 * 23 + (Name?.GetHashCode() ?? 0) * 23 + (Owner?.GetHashCode() ?? 0) * 23 + (LocalPath?.TrimEnd('\\').ToUpperInvariant().GetHashCode() ?? 0);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as LocalRepositoryModel;
            return Equals(other);
        }

        public bool Equals(LocalRepositoryModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null &&
                String.Equals(Name, other.Name) &&
                String.Equals(Owner, other.Owner) &&
                String.Equals(CloneUrl, other.CloneUrl) &&
                String.Equals(LocalPath?.TrimEnd('\\'), other.LocalPath?.TrimEnd('\\'), StringComparison.CurrentCultureIgnoreCase);
        }

        internal string DebuggerDisplay => String.Format(
            CultureInfo.InvariantCulture,
            "{4}\tOwner: {0} Name: {1} CloneUrl: {2} LocalPath: {3}",
            Owner,
            Name,
            CloneUrl,
            LocalPath,
            GetHashCode());

        static string ExtractRepositoryName(string path)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new ArgumentException("Path does not exist", nameof(path));

            var uri = GitService.GitServiceHelper.GetUri(path);
            return uri?.RepositoryName ?? dir.Name;
        }

        static string ExtractCloneUrl(string path)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new ArgumentException("Path does not exist", nameof(path));

            return GitService.GitServiceHelper.GetUri(path);
        }
    }
}
