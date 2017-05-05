using System;

namespace GitHub.Models
{
    public class IssueCommentModel : ICommentModel
    {
        public string Body { get; set; }
        public int Id { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public IAccount User { get; set; }
    }
}
