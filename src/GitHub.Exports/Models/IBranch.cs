using System;
using GitHub.Collections;

namespace GitHub.Models
{
    public interface IBranch : ICopyable<IBranch>,
        IEquatable<IBranch>, IComparable<IBranch>
    {
        string Id { get; }
        string Name { get; }
        IRepositoryModel Repository { get; }
        string DisplayName { get; set; }
    }
}
