using System;

namespace GitHub.Models
{
    /// <summary>
    /// Describes the possible values for <see cref="PullRequestFileModel.Status"/>.
    /// </summary>
    public enum PullRequestFileStatus
    {
        /// <summary>
        /// The file was modified in the pull request.
        /// </summary>
        Modified,

        /// <summary>
        /// The file was added by the pull request.
        /// </summary>
        Added,

        /// <summary>
        /// The file was removed by the pull request.
        /// </summary>
        Removed,

        /// <summary>
        /// The file was moved or renamed by the pull request.
        /// </summary>
        Renamed,
    }

    /// <summary>
    /// Holds details of a file changed by a pull request.
    /// </summary>
    public class PullRequestFileModel
    {
        /// <summary>
        /// Gets or sets the path to the changed file, relative to the repository.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the SHA of the changed file.
        /// </summary>
        public string Sha { get; set; }

        /// <summary>
        /// Gets or sets the status of the changed file (modified, added, removed etc).
        /// </summary>
        public PullRequestFileStatus Status { get; set;  }
    }
}
