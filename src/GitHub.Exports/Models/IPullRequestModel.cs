using System;
using System.Collections.Generic;
using GitHub.Collections;

namespace GitHub.Models
{
    /// TODO: A PullRequestState class already exists hence the ugly naming of this.
    /// Merge the two when the maintainer workflow has been merged to master.
    public enum PullRequestStateEnum
    {
        Open,
        Merged,
        Closed,
    }

    public interface IPullRequestModel : ICopyable<IPullRequestModel>,
        IEquatable<IPullRequestModel>, IComparable<IPullRequestModel>
    {
        int Number { get; }
        string Title { get; }
        PullRequestStateEnum State { get; }
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
        IReadOnlyList<IPullRequestFileModel> ChangedFiles { get; }
        IReadOnlyList<ICommentModel> Comments { get; }
        IReadOnlyList<IPullRequestReviewModel> Reviews { get; set; }
        IReadOnlyList<IPullRequestReviewCommentModel> ReviewComments { get; set; }
    }
}
