using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class PullRequestDetailModel
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public ActorModel Author { get; set; }
        public string Title { get; set; }
        public PullRequestStateEnum State { get; set; }
        public string Body { get; set; }
        public string BaseRefName { get; set; }
        public string BaseRefSha { get; set; }
        public string BaseRepositoryOwner { get; set; }
        public string HeadRefName { get; set; }
        public string HeadRefSha { get; set; }
        public string HeadRepositoryOwner { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public IReadOnlyList<PullRequestFileModel> ChangedFiles { get; set; }
        public IReadOnlyList<PullRequestReviewModel> Reviews { get; set; }
        public IReadOnlyList<PullRequestReviewThreadModel> Threads { get; set; }
    }
}
