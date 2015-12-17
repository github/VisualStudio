using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using GitHub.Primitives;
using GitHub.UI;
using GitHub.VisualStudio.Helpers;
using GitHub.Services;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SimpleRepositoryModel : NotificationAwareObject, ISimpleRepositoryModel, INotifyPropertySource
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

        public string Name { get; }
        UriString cloneUrl;
        public UriString CloneUrl { get { return cloneUrl; } set { cloneUrl = value; this.RaisePropertyChange(); } }
        public string LocalPath { get; }
        Octicon icon;
        public Octicon Icon { get { return icon; } set { icon = value; this.RaisePropertyChange(); } }

        /// <summary>
        /// Implementation
        /// </summary>
        /// <param name="other"></param>
        public void CopyFrom(ISimpleRepositoryModel other)
        {
            if (!Equals(other))
                throw new ArgumentException("Instance to copy from doesn't match this instance. this:(" + this + ") other:(" + other + ")", nameof(other));
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

        bool IEquatable<ISimpleRepositoryModel>.Equals(ISimpleRepositoryModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && String.Equals(Name, other.Name) && String.Equals(CloneUrl, other.CloneUrl) && String.Equals(LocalPath?.TrimEnd('\\'), other.LocalPath?.TrimEnd('\\'), StringComparison.CurrentCultureIgnoreCase);
        }

        public int CompareTo(ISimpleRepositoryModel other)
        {
            return string.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool operator >(SimpleRepositoryModel lhs, SimpleRepositoryModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return lhs?.CompareTo(rhs) > 0;
        }

        public static bool operator <(SimpleRepositoryModel lhs, SimpleRepositoryModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return (object)lhs == null || lhs.CompareTo(rhs) < 0;
        }

        public static bool operator ==(SimpleRepositoryModel lhs, SimpleRepositoryModel rhs)
        {
            return Equals(lhs, rhs) && ((object)lhs == null || lhs.CompareTo(rhs) == 0);
        }

        public static bool operator !=(SimpleRepositoryModel lhs, SimpleRepositoryModel rhs)
        {
            return !(lhs == rhs);
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
