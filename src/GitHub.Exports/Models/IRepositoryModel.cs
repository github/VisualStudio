using GitHub.Collections;
using System;

namespace GitHub.Models
{
    public interface IRepositoryModel : ISimpleRepositoryModel, ICopyable<IRepositoryModel>,
        IEquatable<IRepositoryModel>, IComparable<IRepositoryModel>
    {
        long Id { get; }
        IAccount Owner { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
    }
}
