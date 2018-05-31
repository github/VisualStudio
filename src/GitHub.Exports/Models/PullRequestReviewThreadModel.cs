using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class PullRequestReviewThreadModel
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string CommitSha { get; set; }
        public string DiffHunk { get; set; }
        public bool IsOutdated { get; set; }
        public int? Position { get; set; }
        public int OriginalPosition { get; set; }
        public string OriginalCommitSha { get; set; }
        public IReadOnlyList<PullRequestReviewCommentModel> Comments { get; set; }
    }
}
