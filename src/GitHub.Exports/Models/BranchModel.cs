using System;
using System.Globalization;
using GitHub.Collections;

namespace GitHub.Models
{
    public class BranchModel : ICopyable<BranchModel>,
        IEquatable<BranchModel>, IComparable<BranchModel>
    {
        public BranchModel(string name, RepositoryModel repo, string sha, bool isTracking, string trackedSha, string trackedRemoteName) :
            this(name, repo)
        {
            IsTracking = isTracking;
            Sha = sha;
            TrackedSha = trackedSha;
            TrackedRemoteName = trackedRemoteName;
        }

        public BranchModel(string name, RepositoryModel repo)
        {
            Extensions.Guard.ArgumentNotEmptyString(name, nameof(name));
            Extensions.Guard.ArgumentNotNull(repo, nameof(repo));

            Name = DisplayName = name;
            Repository = repo;
            Id = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Repository.Owner, Name);
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public RepositoryModel Repository { get; private set; }
        public bool IsTracking { get; private set; }
        public string DisplayName { get; set; }
        public string Sha { get; private set; }
        public string TrackedSha { get; private set; }
        public string TrackedRemoteName { get; private set; }

        #region Equality things
        public void CopyFrom(BranchModel other)
        {
            if (!Equals(other))
                throw new ArgumentException("Instance to copy from doesn't match this instance. this:(" + this + ") other:(" + other + ")", nameof(other));
            Id = other.Id;
            Name = other.Name;
            Repository = other.Repository;
            DisplayName = other.DisplayName;
            IsTracking = other.IsTracking;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as BranchModel;
            return other != null && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        bool IEquatable<BranchModel>.Equals(BranchModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Id == other.Id;
        }

        public int CompareTo(BranchModel other)
        {
            return other != null ? string.Compare(Id, other.Id, StringComparison.CurrentCulture) : 1;
        }

        public static bool operator >(BranchModel lhs, BranchModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return lhs?.CompareTo(rhs) > 0;
        }

        public static bool operator <(BranchModel lhs, BranchModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return (object)lhs == null || lhs.CompareTo(rhs) < 0;
        }

        public static bool operator ==(BranchModel lhs, BranchModel rhs)
        {
            return Equals(lhs, rhs) && ((object)lhs == null || lhs.CompareTo(rhs) == 0);
        }

        public static bool operator !=(BranchModel lhs, BranchModel rhs)
        {
            return !(lhs == rhs);
        }
        #endregion
    }
}
