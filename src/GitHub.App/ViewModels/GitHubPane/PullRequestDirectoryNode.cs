using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A directory node in a pull request changes tree.
    /// </summary>
    public class PullRequestDirectoryNode : IPullRequestDirectoryNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDirectoryNode"/> class.
        /// </summary>
        /// <param name="relativePath">The path to the directory, relative to the repository.</param>
        public PullRequestDirectoryNode(string relativePath)
        {
            DirectoryName = System.IO.Path.GetFileName(relativePath);
            RelativePath = relativePath.Replace("/", "\\");
            Directories = new List<IPullRequestDirectoryNode>();
            Files = new List<IPullRequestFileNode>();
        }

        /// <summary>
        /// Gets the name of the directory without path information.
        /// </summary>
        public string DirectoryName { get; }

        /// <summary>
        /// Gets the path to the directory, relative to the root of the repository.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the directory children of the node.
        /// </summary>
        public IList<IPullRequestDirectoryNode> Directories { get; }

        /// <summary>
        /// Gets the file children of the node.
        /// </summary>
        public IList<IPullRequestFileNode> Files { get; }

        /// <summary>
        /// Gets the children of the directory.
        /// </summary>
        public IEnumerable<IPullRequestChangeNode> Children
        {
            get { return Directories.Cast<IPullRequestChangeNode>().Concat(Files); }
        }
    }
}
