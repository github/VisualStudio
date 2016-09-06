using GitHub.Collections;
using System;
using GitHub.UI;

namespace GitHub.Models
{
    public interface IRepositoryModel : IRepositoryModelBase, ICopyable<IRepositoryModel>,
        IEquatable<IRepositoryModel>, IComparable<IRepositoryModel>
    {
        long Id { get; }
        IAccount OwnerAccount { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
        bool IsFork { get; }
        IRepositoryModel Parent { get; }
        IBranch DefaultBranch { get; }
    }
}
