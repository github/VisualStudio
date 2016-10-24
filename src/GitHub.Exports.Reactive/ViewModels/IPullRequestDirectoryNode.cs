using System.Collections.Generic;

namespace GitHub.ViewModels
{
    public interface IPullRequestDirectoryNode : IPullRequestChangeNode
    {
        string DirectoryName { get; }
        IEnumerable<IPullRequestChangeNode> Children { get; }
    }
}