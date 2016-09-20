using System;

namespace GitHub.Models
{
    public interface IPullRequestDetailModel : IPullRequestModel
    {
        string SourceBranchLabel { get; }
        string TargetBranchLabel { get; }
        int CommitCount { get; }
        int FilesChangedCount { get; }
        string Body { get; }
    }
}
