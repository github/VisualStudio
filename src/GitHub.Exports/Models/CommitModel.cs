using System;
using GitHub.Collections;

namespace GitHub.Models
{
    public class CommitModel : ICommitModel
    {
        public CommitModel() { }
        public CommitModel(string message)
        {
            Message = message;
        }
        public string Message { get; }

        int IComparable<ICommitModel>.CompareTo(ICommitModel other)
        {
            throw new NotImplementedException();
        }

        void ICopyable<ICommitModel>.CopyFrom(ICommitModel other)
        {
            throw new NotImplementedException();
        }

        bool IEquatable<ICommitModel>.Equals(ICommitModel other)
        {
            throw new NotImplementedException();
        }
    }
}
