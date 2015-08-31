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
            return (Name?.GetHashCode() ?? 0) ^ (CloneUrl?.GetHashCode() ?? 0) ^ (LocalPath?.TrimEnd('\\').ToUpperInvariant().GetHashCode() ?? 0);
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
