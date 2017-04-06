using System;

namespace GitHub.Models
{
    public interface IPullRequestReviewCommentModel
    {
        string Path { get; }
        int? Position { get; }
        IAccount User { get; }
        string Body { get; }
        DateTimeOffset UpdatedAt { get; }
    }
}
