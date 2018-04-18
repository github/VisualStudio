using System;

namespace GitHub.Models
{
    public class IssueCommentModel : ICommentModel
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public IAccount User { get; set; }
    }
}
