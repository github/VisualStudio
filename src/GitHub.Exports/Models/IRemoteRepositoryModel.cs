using GitHub.Collections;
using System;
using GitHub.UI;

namespace GitHub.Models
{
    public interface IRemoteRepositoryModel : IRepositoryModel, ICopyable<IRemoteRepositoryModel>,
        IEquatable<IRemoteRepositoryModel>, IComparable<IRemoteRepositoryModel>
    {
        long Id { get; }
        IAccount OwnerAccount { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
        bool IsFork { get; }
        IRemoteRepositoryModel Parent { get; }
        IBranch DefaultBranch { get; }
    }
}
