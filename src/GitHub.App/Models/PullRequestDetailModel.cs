using System;
using NullGuard;

namespace GitHub.Models
{
    public class PullRequestDetailModel : PullRequestModel, IPullRequestDetailModel
    {
        public PullRequestDetailModel(int number,
            string title,
            IAccount author,
            [AllowNull]IAccount assignee,
            DateTimeOffset createdAt,
            DateTimeOffset? updatedAt = null)
            : base(number, title, author, assignee, createdAt, updatedAt)
        {
        }

        public string Body { get; set; } = string.Empty;
    }
}
