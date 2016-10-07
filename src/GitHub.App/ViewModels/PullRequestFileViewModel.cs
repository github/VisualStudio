using System;

namespace GitHub.ViewModels
{
    public class PullRequestFileViewModel : IPullRequestFileViewModel
    {
        public PullRequestFileViewModel(string fullPath, bool added, bool deleted)
        {
            Added = added;
            Deleted = deleted;
            FileName = System.IO.Path.GetFileName(fullPath);
            Path = System.IO.Path.GetDirectoryName(fullPath);
        }

        public bool Added { get; }
        public bool Deleted { get; }
        public string FileName { get; }
        public string Path { get; }
    }
}
