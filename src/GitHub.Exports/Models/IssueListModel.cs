using System;
using GitHub.Models;

namespace GitHub.Models
{
    public class IssueListModel
    {
        public string NodeId { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public ActorModel Author { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
