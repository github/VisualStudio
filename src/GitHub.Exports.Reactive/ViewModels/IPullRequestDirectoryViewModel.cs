using System.Collections.Generic;

namespace GitHub.ViewModels
{
    public interface IPullRequestDirectoryViewModel : IPullRequestChangeNode
    {
        string DirectoryName { get; }
        IEnumerable<IPullRequestChangeNode> Children { get; }
    }
}