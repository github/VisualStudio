using System;
using Octokit;

namespace GitHub.Models
{
    public class PullRequestReviewModel : IPullRequestReviewModel
    {
        public long Id { get; set; }
        public string NodeId { get; set; }
        public IAccount User { get; set; }
        public string Body { get; set; }
        public PullRequestReviewState State { get; set; }
        public string CommitId { get; set; }
    }
}
