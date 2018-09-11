using System;

namespace GitHub.Models
{
    /// <summary>
    /// Holds the result of a call to <see cref="IDialogService.ShowCloneDialog"/>.
    /// </summary>
    public class CloneDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloneDialogResult"/> class.
        /// </summary>
        /// <param name="path">The path to clone the repository to.</param>
        /// <param name="repository">The selected repository.</param>
        public CloneDialogResult(string path, IRepositoryModel repository)
        {
            Path = path;
            Repository = repository;
        }

        /// <summary>
        /// Gets the path to clone the repository to.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the repository selected by the user.
        /// </summary>
        public IRepositoryModel Repository { get; }
    }
}
