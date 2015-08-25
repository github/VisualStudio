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
            return (Owner != null ? 0 : Owner.GetHashCode()) ^ base.GetHashCode();
        }

        /// <summary>
        /// This equality comparison will check if the references are the same,
        /// and if they're not, it will explicitely check if the contents of this 
        /// object are the same. If you only want reference checking, call
        /// ReferenceEquals
        /// </summary>
        public static bool operator ==([AllowNull]RepositoryModel lhs, [AllowNull]RepositoryModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if ((object)lhs == null || (object)rhs == null)
                return false;
            return lhs.Owner == rhs.Owner && (lhs as SimpleRepositoryModel) == (rhs as SimpleRepositoryModel);
        }

        /// <summary>
        /// This equality comparison will check if the references are the same,
        /// and if they're not, it will explicitely check if the contents of this 
        /// object are the same. If you only want reference checking, call
        /// ReferenceEquals
        /// </summary>
        public static bool operator !=([AllowNull]RepositoryModel lhs, [AllowNull]RepositoryModel rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals([AllowNull]object other)
        {
            return other != null && this == other as RepositoryModel;
        }

        bool IEquatable<RepositoryModel>.Equals([AllowNull]RepositoryModel other)
        {
            return other != null && this == other;
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
