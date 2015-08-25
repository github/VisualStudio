using GitHub.Primitives;
using GitHub.UI;
using GitHub.VisualStudio.Helpers;
using System;
using System.Diagnostics;
using System.Globalization;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SimpleRepositoryModel : NotificationAwareObject, ISimpleRepositoryModel, INotifyPropertySource, IEquatable<SimpleRepositoryModel>
    {
        public SimpleRepositoryModel(string name, UriString cloneUrl, string localPath = null)
        {
            Name = name;
            CloneUrl = cloneUrl;
            LocalPath = localPath;
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

        public string Name { get; private set; }
        public UriString CloneUrl { get; private set; }
        public string LocalPath { get; private set; }
        Octicon icon;
        public Octicon Icon { get { return icon; } set { icon = value; this.RaisePropertyChange(); } }

        public override int GetHashCode()
        {
            return (Name == null ? 0 : Name.GetHashCode()) ^ (CloneUrl == null ? 0 : CloneUrl.GetHashCode()) ^ (LocalPath == null ? 0 : LocalPath.TrimEnd('\\').ToUpperInvariant().GetHashCode());
        }

        /// <summary>
        /// This equality comparison will check if the references are the same,
        /// and if they're not, it will explicitely check if the contents of this 
        /// object are the same. If you only want reference checking, call
        /// ReferenceEquals
        /// </summary>
        public static bool operator==(SimpleRepositoryModel lhs, SimpleRepositoryModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if ((object)lhs == null || (object)rhs == null)
                return false;
            return String.Equals(lhs.Name, rhs.Name) && String.Equals(lhs.CloneUrl, rhs.CloneUrl) && String.Equals(lhs.LocalPath?.TrimEnd('\\'), rhs.LocalPath?.TrimEnd('\\'), StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// This equality comparison will check if the references are the same,
        /// and if they're not, it will explicitely check if the contents of this 
        /// object are the same. If you only want reference checking, call
        /// ReferenceEquals
        /// </summary>
        public static bool operator!=(SimpleRepositoryModel lhs, SimpleRepositoryModel rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            return other != null && Equals(other as SimpleRepositoryModel);
        }

        bool IEquatable<SimpleRepositoryModel>.Equals(SimpleRepositoryModel other)
        {
            return other != null && this == other;
        }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,
                    "{3}\tName: {0} CloneUrl: {1} LocalPath: {2}", Name, CloneUrl, LocalPath, GetHashCode());
            }
        }
    }
}
