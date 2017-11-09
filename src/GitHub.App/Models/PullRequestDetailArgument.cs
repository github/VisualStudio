using System;
using GitHub.ViewModels;
using GitHub.Primitives;

namespace GitHub.Models
{
    /// <summary>
    /// Passes arguments to a <see cref="PullRequestDetailViewModel"/>
    /// </summary>
    public class PullRequestDetailArgument
    {
        /// <summary>
        /// Gets or sets the owner of the repository containing the pull request.
        /// </summary>
        public string RepositoryOwner { get; set; }

        /// <summary>
        /// Gets or sets the number of the pull request.
        /// </summary>
        public int Number { get; set; }
    }
}
