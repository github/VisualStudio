using System;
using System.Collections.Generic;
using GitHub.Collections;

namespace GitHub.Models
{
    public enum PullRequestState
    {
        Open,
        Closed,
        Merged,
    }

    public enum PullRequestChecksState
    {
        None,
        Pending,
        Success,
        Failure
    }

    public interface IPullRequestModel : ICopyable<IPullRequestModel>,
        IEquatable<IPullRequestModel>, IComparable<IPullRequestModel>
    {
        int Number { get; }
        string Title { get; }
        PullRequestState State { get; }
        int CommentCount { get; }
        int CommitCount { get; }
        bool IsOpen { get; }
        bool Merged { get; }
        bool HasNewComments { get; }
        GitReferenceModel Base { get; }
        GitReferenceModel Head { get; }
        string Body { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
        IAccount Author { get; }
        IAccount Assignee { get; }
    }
}
