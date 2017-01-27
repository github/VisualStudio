using System.Collections.Generic;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a directory node in a pull request changes tree.
    /// </summary>
    public interface IPullRequestDirectoryNode : IPullRequestChangeNode
    {
        /// <summary>
        /// Gets the name of the directory without path information.
        /// </summary>
        string DirectoryName { get; }

        /// <summary>
        /// Gets the children of the directory.
        /// </summary>
        IEnumerable<IPullRequestChangeNode> Children { get; }
    }
}