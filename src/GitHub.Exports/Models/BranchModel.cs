using System;
using System.Globalization;

namespace GitHub.Models
{
    public class BranchModel : IBranch
    {
        public BranchModel(string name, IRepositoryModel repo)
        {
            Extensions.Guard.ArgumentNotEmptyString(name, nameof(name));
            Extensions.Guard.ArgumentNotNull(repo, nameof(repo));

            Name = DisplayName = name;
            Repository = repo;
            Id = String.Format(CultureInfo.InvariantCulture, "{0}/{1}", Repository.Owner, Name);
        }

        public BranchModel(Octokit.Branch branch, IRepositoryModel repo)
        {
            Extensions.Guard.ArgumentNotNull(branch, nameof(branch));
            Extensions.Guard.ArgumentNotNull(repo, nameof(repo));

            Name = DisplayName = branch.Name;
            Repository = repo;
            Id = String.Format(CultureInfo.InvariantCulture, "{0}/{1}", Repository.Owner, Name);
        }

        public BranchModel(LibGit2Sharp.Branch branch, IRepositoryModel repo)
        {
            Extensions.Guard.ArgumentNotNull(branch, nameof(branch));
            Extensions.Guard.ArgumentNotNull(repo, nameof(repo));

            Name = DisplayName = branch.FriendlyName;
            Repository = branch.IsRemote ? new LocalRepositoryModel(branch.Remote.Url) : repo;
            IsTracking = branch.IsTracking;
            Id = String.Format(CultureInfo.InvariantCulture, "{0}/{1}", Repository.Owner, Name);
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public IRepositoryModel Repository { get; private set; }
        public bool IsTracking { get; private set; }
        public string DisplayName { get; set; }


        #region Equality things
        public void CopyFrom(IBranch other)
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

        bool IEquatable<IBranch>.Equals(IBranch other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Id == other.Id;
        }

        public int CompareTo(IBranch other)
        {
            return other != null ? String.Compare(Id, other.Id, StringComparison.CurrentCulture) : 1;
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
