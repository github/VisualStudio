using GitHub.Primitives;
using GitHub.UI;
using GitHub.VisualStudio.Helpers;
using System;
using System.Diagnostics;
using System.Globalization;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SimpleRepositoryModel : NotificationAwareObject, ISimpleRepositoryModel, INotifyPropertySource, IEquatable<ISimpleRepositoryModel>
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

        public bool Equals(ISimpleRepositoryModel other)
        {
            if (Object.ReferenceEquals(other, null)) return false;
            if (Object.ReferenceEquals(this, other)) return true;
            return Name.Equals(other.Name) && String.Equals(LocalPath, other.LocalPath, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return (Name == null ? 0 : Name.GetHashCode()) ^ (LocalPath == null ? 0 : LocalPath.TrimEnd('\\').ToUpperInvariant().GetHashCode());
        }

        public static bool operator==(SimpleRepositoryModel lhs, SimpleRepositoryModel rhs)
        {
            if ((object)lhs == null && (object)rhs == null)
                return true;
            if ((object)lhs != null && (object)rhs != null)
                return lhs.Equals(rhs);
            return false;
        }

        public static bool operator!=(SimpleRepositoryModel lhs, SimpleRepositoryModel rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            var o = obj as ISimpleRepositoryModel;
            if (o != null)
                return Equals(o);
            return false;
        }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,
                    "{3}: Name: {0} CloneUrl: {1} LocalPath: {2}", Name, CloneUrl, LocalPath, GetHashCode());
            }
        }
    }
}
