using System;
using GitHub.Collections;

namespace GitHub.Models
{
    public interface IBranch:ICopyable<IBranch>,
        IEquatable<IBranch>, IComparable<IBranch>
    {
        string Name { get; }
    }
}
