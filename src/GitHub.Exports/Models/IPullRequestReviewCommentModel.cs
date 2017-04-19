using System;

namespace GitHub.Models
{
    public interface IPullRequestReviewCommentModel
    {
        int Id { get; }
        string Path { get; }
        int? Position { get; }
        int? OriginalPosition { get; }
        string CommitId { get; }
        string OriginalCommitId { get; }
        string DiffHunk { get; }
        IAccount User { get; }
        string Body { get; }
        DateTimeOffset UpdatedAt { get; }
    }
}
