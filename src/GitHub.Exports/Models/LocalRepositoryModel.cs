using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using GitHub.Primitives;
using GitHub.UI;
using GitHub.Exports;
using GitHub.Services;
using GitHub.Extensions;
using System.Threading.Tasks;

namespace GitHub.Models
{
    /// <summary>
    /// A locally cloned repository.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LocalRepositoryModel : RepositoryModel, ILocalRepositoryModel, IEquatable<LocalRepositoryModel>
    {
        readonly IGitService gitService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRepositoryModel"/> class.
        /// </summary>
        /// <param name="name">The repository name.</param>
        /// <param name="cloneUrl">The repository's clone URL.</param>
        /// <param name="localPath">The repository's local path.</param>
        /// <param name="gitService">The service used to refresh the repository's URL.</param>
        public LocalRepositoryModel(string name, UriString cloneUrl, string localPath, IGitService gitService)
            : base(name, cloneUrl)
        {
            Guard.ArgumentNotEmptyString(localPath, nameof(localPath));
            Guard.ArgumentNotNull(gitService, nameof(gitService));

            this.gitService = gitService;
            LocalPath = localPath;
            Icon = Octicon.repo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRepositoryModel"/> class.
        /// </summary>
        /// <param name="path">The repository's local path.</param>
        /// <param name="gitService">The service used to find the repository's URL.</param>
        public LocalRepositoryModel(string path, IGitService gitService)
            : base(path, gitService)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));

            this.gitService = gitService;
            LocalPath = path;
            Icon = Octicon.repo;
        }

        /// <summary>
        /// Updates the clone URL from the local repository.
        /// </summary>
        public void Refresh()
        {
            if (LocalPath == null)
                return;
            CloneUrl = gitService.GetUri(LocalPath);
        }

        /// <summary>
        /// Generates a http(s) url to the repository in the remote server, optionally
        /// pointing to a specific file and specific line range in it.
        /// </summary>
        /// <param name="linkType">Type of link to generate</param>
        /// <param name="path">The file to generate an url to. Optional.</param>
        /// <param name="startLine">A specific line, or (if specifying the <paramref name="endLine"/> as well) the start of a range</param>
        /// <param name="endLine">The end of a line range on the specified file.</param>
        /// <returns>An UriString with the generated url, or null if the repository has no remote server configured or if it can't be found locally</returns>
        public async Task<UriString> GenerateUrl(LinkType linkType, string path = null, int startLine = -1, int endLine = -1)
        {
            if (CloneUrl == null)
                return null;

            var sha = await gitService.GetLatestPushedSha(path ?? LocalPath);
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

            return new UriString(GenerateUrl(linkType, CloneUrl.ToRepositoryUrl().AbsoluteUri, sha, path, startLine, endLine));
        }

        const string CommitFormat = "{0}/commit/{1}";
        const string BlobFormat = "{0}/blob/{1}/{2}";
        const string BlameFormat = "{0}/blame/{1}/{2}";
        const string StartLineFormat = "{0}#L{1}";
        const string EndLineFormat = "{0}-L{1}";
        static string GenerateUrl(LinkType linkType, string basePath, string sha, string path, int startLine = -1, int endLine = -1)
        {
            if (sha == null)
                return basePath;

            if (String.IsNullOrEmpty(path))
                return String.Format(CultureInfo.InvariantCulture, CommitFormat, basePath, sha);

            var ret = String.Format(CultureInfo.InvariantCulture, GetLinkFormat(linkType), basePath, sha, path.Replace(@"\", "/"));

            if (startLine < 0)
                return ret;
            ret = String.Format(CultureInfo.InvariantCulture, StartLineFormat, ret, startLine);
            if (endLine < 0)
                return ret;
            return String.Format(CultureInfo.InvariantCulture, EndLineFormat, ret, endLine);
        }

        /// <summary>
        /// Selects the proper format for the link type, defaults to the blob url when link type is not selected.
        /// </summary>
        /// <param name="linkType">Type of link to generate</param>
        /// <returns>The string format of the selected link type</returns>
        static string GetLinkFormat(LinkType linkType)
        {
            switch (linkType)
            {
                case LinkType.Blame:
                    return BlameFormat;

                case LinkType.Blob:
                    return BlobFormat;

                default:
                    return BlobFormat;
            }
        }

        /// <summary>
        /// Gets the local path of the repository.
        /// </summary>
        public string LocalPath { get; }

        /// <summary>
        /// Gets the head SHA of the repository.
        /// </summary>
        public string HeadSha
        {
            get
            {
                using (var repo = gitService.GetRepository(LocalPath))
                {
                    return repo?.Commits.FirstOrDefault()?.Sha ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the current branch of the repository.
        /// </summary>
        public IBranch CurrentBranch
        {
            get
            {
                // BranchModel doesn't keep a reference to Repository
                using (var repo = gitService.GetRepository(LocalPath))
                {
                    return new LocalBranchModel(repo?.Head, this, gitService);
                }
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
    }
}
