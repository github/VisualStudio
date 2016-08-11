using GitHub.Collections;
using System;

namespace GitHub.Models
{
    public interface IBranch : ICopyable<IBranch>,
        IEquatable<IBranch>, IComparable<IBranch>
    {
        string Id { get; }
        string Name { get; }
        ISimpleRepositoryModel Repository { get; set; }
        bool IsTracking { get; set; }
        string DisplayName { get; set; }
    }
}
