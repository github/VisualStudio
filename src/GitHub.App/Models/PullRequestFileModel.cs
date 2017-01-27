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

        public string FileName { get; set; }
        public string Sha { get; set; }
        public PullRequestFileStatus Status { get; set; }
    }
}
