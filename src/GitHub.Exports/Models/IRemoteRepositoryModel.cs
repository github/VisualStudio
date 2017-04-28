using GitHub.Collections;
using System;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a repository read from the GitHub API.
    /// </summary>
    public interface IRemoteRepositoryModel : IRepositoryModel, ICopyable<IRemoteRepositoryModel>,
        IEquatable<IRemoteRepositoryModel>, IComparable<IRemoteRepositoryModel>
    {
        /// <summary>
        /// Gets the repository's API ID.
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Gets the account that is the ower of the repository.
        /// </summary>
        IAccount OwnerAccount { get; }

        /// <summary>
        /// Gets the date and time at which the repository was created.
        /// </summary>
        DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets the repository's last update date and time.
        /// </summary>
        DateTimeOffset UpdatedAt { get; }

        /// <summary>
        /// Gets a value indicating whether the repository is a fork.
        /// </summary>
        bool IsFork { get; }

        /// <summary>
        /// Gets the repository from which this repository was forked, if any.
        /// </summary>
        IRemoteRepositoryModel Parent { get; }

        /// <summary>
        /// Gets the default branch for the repository.
        /// </summary>
        IBranch DefaultBranch { get; }
    }
}
