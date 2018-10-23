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

        public LocalRepositoryModel()
        {
        }

        /// <summary>
        /// Gets the local path of the repository.
        /// </summary>
        public string LocalPath { get; set; }

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
                    return new BranchModel(repo?.Head, this, gitService);
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
