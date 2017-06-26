using System;
using GitHub.ViewModels;

namespace GitHub.Models
{
    /// <summary>
    /// Passes arguments to a <see cref="PullRequestDetailViewModel"/>
    /// </summary>
    public class PullRequestDetailArgument
    {
        /// <summary>
        /// Gets or sets the repository containing the pull request.
        /// </summary>
        public IRemoteRepositoryModel Repository { get; set; }

        /// <summary>
        /// Gets or sets the number of the pull request.
        /// </summary>
        public int Number { get; set; }
    }
}
