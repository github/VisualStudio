using System;
using GitHub.Collections;

namespace GitHub.Models
{
    public interface IPullRequestModel : ICopyable<IPullRequestModel>,
        IEquatable<IPullRequestModel>, IComparable<IPullRequestModel>
    {
        int Number { get; }
        string Title { get; }
        int CommentCount { get; }
        bool IsOpen { get; }
        bool HasNewComments { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
        IAccount Author { get; }
        IAccount Assignee { get; }
        ISimpleRepositoryModel Repository { get; }
        IBranch Head { get; }
        IBranch Base { get; }
    }
}
