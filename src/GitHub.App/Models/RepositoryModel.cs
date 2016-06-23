using GitHub.Primitives;
using NullGuard;
using System;
using System.Globalization;

namespace GitHub.Models
{
    public class RepositoryModel : SimpleRepositoryModel, IRepositoryModel,
        IEquatable<RepositoryModel>, IComparable<RepositoryModel>
    {
        public RepositoryModel(long id, string name, UriString cloneUrl, bool isPrivate, bool isFork,  IAccount ownerAccount)
            : base(name, cloneUrl)
        {
            Id = id;
            Owner = ownerAccount;
            SetIcon(isPrivate, isFork);
        }

        public void CopyFrom(IRepositoryModel other)
        {
            if (!Equals(other))
                throw new ArgumentException("Instance to copy from doesn't match this instance. this:(" + this + ") other:(" + other + ")", nameof(other));
            Icon = other.Icon;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals([AllowNull]object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as RepositoryModel;
            return other != null && Id == other.Id;
        }

        bool IEquatable<IRepositoryModel>.Equals([AllowNull]IRepositoryModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Id == other.Id;
        }

        bool IEquatable<RepositoryModel>.Equals([AllowNull]RepositoryModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Id == other.Id;
        }

        public int CompareTo([AllowNull]IRepositoryModel other)
        {
            return other != null ? UpdatedAt.CompareTo(other.UpdatedAt) : 1;
        }

        public int CompareTo([AllowNull]RepositoryModel other)
        {
            return other != null ? UpdatedAt.CompareTo(other.UpdatedAt) : 1;
        }

        public static bool operator >([AllowNull]RepositoryModel lhs, [AllowNull]RepositoryModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return lhs?.CompareTo(rhs) > 0;
        }

        public static bool operator <([AllowNull]RepositoryModel lhs, [AllowNull]RepositoryModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return (object)lhs == null || lhs.CompareTo(rhs) < 0;
        }

        public static bool operator ==([AllowNull]RepositoryModel lhs, [AllowNull]RepositoryModel rhs)
        {
            return ReferenceEquals(lhs, rhs);
        }

        public static bool operator !=([AllowNull]RepositoryModel lhs, [AllowNull]RepositoryModel rhs)
        {
            return !(lhs == rhs);
        }

        public IAccount Owner { get; }
        public long Id { get; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,
                    "{5}\tId: {0} Name: {1} CloneUrl: {2} LocalPath: {3} Account: {4}", Id, Name, CloneUrl, LocalPath, Owner, GetHashCode());
            }
        }
    }
}
