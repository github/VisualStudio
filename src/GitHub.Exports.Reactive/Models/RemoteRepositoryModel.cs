using System;
using System.Globalization;
using GitHub.Primitives;
using GitHub.Extensions;
using GitHub.Collections;

namespace GitHub.Models
{
    /// <summary>
    /// A repository read from the GitHub API.
    /// </summary>
    public class RemoteRepositoryModel : RepositoryModel, ICopyable<RemoteRepositoryModel>,
        IEquatable<RemoteRepositoryModel>, IComparable<RemoteRepositoryModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteRepositoryModel"/> class.
        /// </summary>
        /// <param name="id">The API ID of the repository.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="cloneUrl">The repository's clone URL.</param>
        /// <param name="isPrivate">Whether the repository is private.</param>
        /// <param name="isFork">Whether the repository is a fork.</param>
        /// <param name="ownerAccount">The repository owner account.</param>
        /// <param name="parent">The parent repository if this repository is a fork.</param>
        /// <param name="defaultBranchName">The default branch name (or "master" if undefined).</param>
        public RemoteRepositoryModel(long id, string name, UriString cloneUrl, bool isPrivate, bool isFork, IAccount ownerAccount,
            RemoteRepositoryModel parent, string defaultBranchName = "master")
            : base(name, cloneUrl)
        {
            Guard.ArgumentNotEmptyString(name, nameof(name));
            Guard.ArgumentNotNull(ownerAccount, nameof(ownerAccount));

            Id = id;
            OwnerAccount = ownerAccount;
            IsFork = isFork;
            SetIcon(isPrivate, isFork);
            DefaultBranch = new BranchModel(defaultBranchName, this);
            Parent = parent;
        }

        /// <summary>
        /// This is used by <see cref="RemoteRepositoryModelDesigner"/>.
        /// </summary>
        protected RemoteRepositoryModel()
        {
        }

        #region Equality Things
        public void CopyFrom(RemoteRepositoryModel other)
        {
            if (!Equals(other))
                throw new ArgumentException("Instance to copy from doesn't match this instance. this:(" + this + ") other:(" + other + ")", nameof(other));
            Icon = other.Icon;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as RemoteRepositoryModel;
            return Equals(other);
        }

        public bool Equals(RemoteRepositoryModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Id == other.Id;
        }

        public int CompareTo(RemoteRepositoryModel other)
        {
            return other != null ? UpdatedAt.CompareTo(other.UpdatedAt) : 1;
        }

        public static bool operator >(RemoteRepositoryModel lhs, RemoteRepositoryModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return lhs?.CompareTo(rhs) > 0;
        }

        public static bool operator <(RemoteRepositoryModel lhs, RemoteRepositoryModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return (object)lhs == null || lhs.CompareTo(rhs) < 0;
        }

        public static bool operator ==(RemoteRepositoryModel lhs, RemoteRepositoryModel rhs)
        {
            return ReferenceEquals(lhs, rhs);
        }

        public static bool operator !=(RemoteRepositoryModel lhs, RemoteRepositoryModel rhs)
        {
            return !(lhs == rhs);
        }
        #endregion

        /// <summary>
        /// Gets the account that is the ower of the repository.
        /// </summary>
        public IAccount OwnerAccount { get; }

        /// <summary>
        /// Gets the repository's API ID.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Gets the date and time at which the repository was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets the repository's last update date and time.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Gets a value indicating whether the repository is a fork.
        /// </summary>
        public bool IsFork { get; }

        /// <summary>
        /// Gets the repository from which this repository was forked, if any.
        /// </summary>
        public RemoteRepositoryModel Parent { get; }

        /// <summary>
        /// Gets the default branch for the repository.
        /// </summary>
        public BranchModel DefaultBranch { get; }

        internal string DebuggerDisplay
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture,
                    "{4}\tId: {0} Name: {1} CloneUrl: {2} Account: {3}", Id, Name, CloneUrl, Owner, GetHashCode());
            }
        }
    }
}
