using System;
using GitHub.Collections;
using Octokit;

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
        GitReference Base { get; }
        GitReference Head { get; }
    }
}
