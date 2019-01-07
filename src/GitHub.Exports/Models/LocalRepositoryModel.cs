using System;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using GitHub.Primitives;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a locally cloned repository.
    /// </summary>
    /// <summary>
    /// A locally cloned repository.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LocalRepositoryModel : RepositoryModel, INotifyPropertyChanged, IEquatable<LocalRepositoryModel>
    {
        /// <summary>
        /// Gets the local path of the repository.
        /// </summary>
        public string LocalPath
        {
            get; set;
        }

        /// <summary>
        /// True if repository has remotes but none are named "origin".
        /// </summary>
        public bool HasRemotesButNoOrigin
        {
            get; set;
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
                string.Equals(Name, other.Name, StringComparison.Ordinal) &&
                string.Equals(Owner, other.Owner, StringComparison.Ordinal) &&
                string.Equals(CloneUrl, other.CloneUrl, StringComparison.Ordinal) &&
                string.Equals(LocalPath?.TrimEnd('\\'), other.LocalPath?.TrimEnd('\\'), StringComparison.CurrentCultureIgnoreCase);
        }

        internal string DebuggerDisplay => string.Format(
            CultureInfo.InvariantCulture,
            "{4}\tOwner: {0} Name: {1} CloneUrl: {2} LocalPath: {3}",
            Owner,
            Name,
            CloneUrl,
            LocalPath,
            GetHashCode());
    }
}
