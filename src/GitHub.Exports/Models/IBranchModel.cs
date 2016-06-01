using System;
using GitHub.Collections;

namespace GitHub.Models
{
    public interface IBranchModel:ICopyable<IBranchModel>,
        IEquatable<IBranchModel>, IComparable<IBranchModel>
    {
        string Name { get; }
    }
}
