using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Models
{
    public enum IssueState
    {
        Open,
        Closed,
    }

    public class IssueListModel
    {
        public string NodeId { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public IssueState State { get; set; }
        public ActorModel Author { get; set; }
        public IList<ActorModel> Assignees { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
