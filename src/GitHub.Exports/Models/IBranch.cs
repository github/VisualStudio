using GitHub.Collections;
using System;

namespace GitHub.Models
{
    public interface IBranch : ICopyable<IBranch>,
        IEquatable<IBranch>, IComparable<IBranch>
    {
        string Id { get; }
        string Name { get; }
        IRepositoryModelBase Repository { get; }
        bool IsTracking { get; }
        string DisplayName { get; set; }
    }
}
