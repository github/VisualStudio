using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A directory node in a pull request changes tree.
    /// </summary>
    public class PullRequestDirectoryViewModel : IPullRequestDirectoryViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDirectoryViewModel"/> class.
        /// </summary>
        /// <param name="path">The path to the directory, relative to the repository.</param>
        public PullRequestDirectoryViewModel(string path)
        {
            DirectoryName = System.IO.Path.GetFileName(path);
            Path = path;
            Directories = new List<IPullRequestDirectoryViewModel>();
            Files = new List<IPullRequestFileViewModel>();
        }

        /// <summary>
        /// Gets the name of the directory without path information.
        /// </summary>
        public string DirectoryName { get; }

        /// <summary>
        /// Gets the path to the directory, relative to the root of the repository.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the directory children of the node.
        /// </summary>
        public IList<IPullRequestDirectoryViewModel> Directories { get; }

        /// <summary>
        /// Gets the file children of the node.
        /// </summary>
        public IList<IPullRequestFileViewModel> Files { get; }

        /// <summary>
        /// Gets the children of the directory.
        /// </summary>
        public IEnumerable<IPullRequestChangeNode> Children
        {
            get { return Directories.Cast<IPullRequestChangeNode>().Concat(Files); }
        }
    }
}
