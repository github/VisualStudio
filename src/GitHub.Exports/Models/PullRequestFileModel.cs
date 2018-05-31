using System;

namespace GitHub.Models
{
    public enum PullRequestFileStatus
    {
        Modified,
        Added,
        Removed,
        Renamed,
    }

    public class PullRequestFileModel
    {
        public string FileName { get; set; }
        public string Sha { get; set; }
        public PullRequestFileStatus Status { get; set;  }
    }
}
