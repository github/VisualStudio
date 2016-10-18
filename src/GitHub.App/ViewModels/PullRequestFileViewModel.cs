using System;
using GitHub.Models;

namespace GitHub.ViewModels
{
    public class PullRequestFileViewModel : IPullRequestFileViewModel
    {
        public PullRequestFileViewModel(string fullPath, PullRequestFileStatus status)
        {
            FileName = System.IO.Path.GetFileName(fullPath);
            Path = System.IO.Path.GetDirectoryName(fullPath);
            Status = status;
        }

        public string FileName { get; }
        public string Path { get; }
        public PullRequestFileStatus Status { get; }
    }
}
