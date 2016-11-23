using System;

namespace GitHub.Models
{
    public class PullRequestFileModel : IPullRequestFileModel
    {
        public PullRequestFileModel(string fileName, PullRequestFileStatus status)
        {
            FileName = fileName;
            Status = status;
        }

        public string FileName { get; set; }
        public PullRequestFileStatus Status { get; set; }
    }
}
