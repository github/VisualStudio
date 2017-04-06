using System;

namespace GitHub.Models
{
    public class PullRequestFileModel : IPullRequestFileModel
    {
        public PullRequestFileModel(string fileName, string sha, PullRequestFileStatus status)
        {
            FileName = fileName;
            Sha = sha;
            Status = status;
        }

        public string FileName { get; }
        public string Sha { get; }
        public PullRequestFileStatus Status { get; }
    }
}
