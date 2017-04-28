using System;
using GitHub.Services;

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
        /// <param name="basePath">The selected base path for the clone.</param>
        /// <param name="repository">The selected repository.</param>
        public CloneDialogResult(string basePath, IRepositoryModel repository)
        {
            BasePath = basePath;
            Repository = repository;
        }

        /// <summary>
        /// Gets the filesystem path to which the user wants to clone.
        /// </summary>
        public string BasePath { get; }

        /// <summary>
        /// Gets the repository selected by the user.
        /// </summary>
        public IRepositoryModel Repository { get; }
    }
}
