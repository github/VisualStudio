using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using GitHub.Primitives;
using GitHub.UI;
using GitHub.VisualStudio.Helpers;
using GitHub.Services;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SimpleRepositoryModel : NotificationAwareObject, ISimpleRepositoryModel, IEquatable<SimpleRepositoryModel>
    {
        public SimpleRepositoryModel(string name, UriString cloneUrl, string localPath = null)
        {
            Name = name;
            CloneUrl = cloneUrl;
            LocalPath = localPath;
            Icon = Octicon.repo;
        }

        public SimpleRepositoryModel(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new ArgumentException("Path does not exist", nameof(path));
            var uri = GitService.GitServiceHelper.GetUri(path);
            var name = uri?.NameWithOwner ?? dir.Name;
            Name = name;
            LocalPath = path;
            CloneUrl = uri;
            Icon = Octicon.repo;
        }

        public void SetIcon(bool isPrivate, bool isFork)
        {
            Icon = isPrivate
                    ? Octicon.@lock
                    : isFork
                        ? Octicon.repo_forked
                        : Octicon.repo;
        }

        public void Refresh()
        {
            if (LocalPath == null)
                return;
            var uri = GitService.GitServiceHelper.GetUri(LocalPath);
            if (CloneUrl != uri)
                CloneUrl = uri;
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

        public string Name { get; }
        UriString cloneUrl;
        public UriString CloneUrl { get { return cloneUrl; } set { cloneUrl = value; this.RaisePropertyChange(); } }
        public string LocalPath { get; }
        Octicon icon;
        public Octicon Icon { get { return icon; } set { icon = value; this.RaisePropertyChange(); } }

        public string HeadSha
        {
            get
            {
                var repo = GitService.GitServiceHelper.GetRepo(LocalPath);
                return repo?.Commits.FirstOrDefault()?.Sha ?? String.Empty;
            }
        }

        /// <summary>
        /// Note: We don't consider CloneUrl a part of the hash code because it can change during the lifetime
        /// of a repository. Equals takes care of any hash collisions because of this
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Name?.GetHashCode() ?? 0) ^ (LocalPath?.TrimEnd('\\').ToUpperInvariant().GetHashCode() ?? 0);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as SimpleRepositoryModel;
            return other != null && String.Equals(Name, other.Name) && String.Equals(CloneUrl, other.CloneUrl) && String.Equals(LocalPath?.TrimEnd('\\'), other.LocalPath?.TrimEnd('\\'), StringComparison.CurrentCultureIgnoreCase);
        }

        bool IEquatable<SimpleRepositoryModel>.Equals(SimpleRepositoryModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && String.Equals(Name, other.Name) && String.Equals(CloneUrl, other.CloneUrl) && String.Equals(LocalPath?.TrimEnd('\\'), other.LocalPath?.TrimEnd('\\'), StringComparison.CurrentCultureIgnoreCase);
        }

        internal string DebuggerDisplay => String.Format(
            CultureInfo.InvariantCulture,
            "{3}\tName: {0} CloneUrl: {1} LocalPath: {2}",
            Name,
            CloneUrl,
            LocalPath,
            GetHashCode());
    }
}
