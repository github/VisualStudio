using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.ViewModels
{
    public class PullRequestDirectoryViewModel : IPullRequestDirectoryViewModel
    {
        public PullRequestDirectoryViewModel(string fullPath)
        {
            DirectoryName = System.IO.Path.GetFileName(fullPath);
            Path = fullPath;
            Directories = new List<IPullRequestDirectoryViewModel>();
            Files = new List<IPullRequestFileViewModel>();
        }

        public string DirectoryName { get; }
        public string Path { get; }
        public IList<IPullRequestDirectoryViewModel> Directories { get; }
        public IList<IPullRequestFileViewModel> Files { get; }

        public IEnumerable<IPullRequestChangeNode> Children
        {
            get { return Directories.Cast<IPullRequestChangeNode>().Concat(Files); }
        }
    }
}
