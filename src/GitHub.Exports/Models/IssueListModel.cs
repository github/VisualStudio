using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public enum IssueState
    {
        Open,
        Closed,
    }

    public class IssueListModel
    {
        public ActorModel Author { get; set; }
        public IList<ActorModel> Assignees { get; set; }
        public int CommentCount { get; set; }
        public IList<IssueLabelModel> Labels { get; set; }
        public string NodeId { get; set; }
        public int Number { get; set; }
        public IssueState State { get; set; }
        public string Title { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
