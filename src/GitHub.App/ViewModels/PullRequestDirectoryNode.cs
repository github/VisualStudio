using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.ViewModels
{
    public class PullRequestDirectoryNode : IPullRequestDirectoryNode
    {
        public PullRequestDirectoryNode(string fullPath)
        {
            DirectoryName = System.IO.Path.GetFileName(fullPath);
            Path = fullPath;
            Directories = new List<IPullRequestDirectoryNode>();
            Files = new List<IPullRequestFileNode>();
        }

        public string DirectoryName { get; }
        public string Path { get; }
        public IList<IPullRequestDirectoryNode> Directories { get; }
        public IList<IPullRequestFileNode> Files { get; }

        public IEnumerable<IPullRequestChangeNode> Children
        {
            get { return Directories.Cast<IPullRequestChangeNode>().Concat(Files); }
        }
    }
}
