using GitHub.Collections;
using System;

namespace GitHub.Models
{
    public interface IBranch : ICopyable<IBranch>,
        IEquatable<IBranch>, IComparable<IBranch>
    {
        string Id { get; }
        string Name { get; }
        IRepositoryModel Repository { get; }
        bool IsTracking { get; }
        string DisplayName { get; set; }
    }
}
