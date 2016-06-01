using System;
using GitHub.Collections;

namespace GitHub.Models
{
    public interface ICommitModel : ICopyable<ICommitModel>,
        IEquatable<ICommitModel>, IComparable<ICommitModel>
    {
        string Message { get; }
    }
}
