using GitHub.Primitives;
using NullGuard;
using System;
using System.Globalization;

namespace GitHub.Models
{
    public class RepositoryModel : SimpleRepositoryModel, IRepositoryModel, IEquatable<RepositoryModel>
    {
        public RepositoryModel(string name, UriString cloneUrl, bool isPrivate, bool isFork,  IAccount ownerAccount)
            : base(name, cloneUrl)
        {
            Owner = ownerAccount;
            SetIcon(isPrivate, isFork);
        }

        public IAccount Owner { get; private set; }
        public override int GetHashCode()
        {
            return (Owner?.GetHashCode() ?? 0) ^ base.GetHashCode();
        }

        public override bool Equals([AllowNull]object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as RepositoryModel;
            return other != null && Equals(Owner, other.Owner) && base.Equals(obj);
        }

        bool IEquatable<RepositoryModel>.Equals([AllowNull]RepositoryModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Equals(Owner, other.Owner) && base.Equals(other as SimpleRepositoryModel);
        }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,
                    "{4}\tName: {0} CloneUrl: {1} LocalPath: {2} Account: {3}", Name, CloneUrl, LocalPath, Owner, GetHashCode());
            }
        }
    }
}
