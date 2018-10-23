using System;
using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a repository, either local or retreived via the GitHub API.
    /// </summary>
    public interface IRepositoryModel
    {
        /// <summary>
        /// Gets the name of the repository.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the repository clone URL.
        /// </summary>
        UriString CloneUrl { get; set; }

        /// <summary>
        /// Gets the name of the owner of the repository, taken from the clone URL.
        /// </summary>
        string Owner { get; }

        /// <summary>
        /// Gets an icon for the repository that displays its private and fork state.
        /// </summary>
        Octicon Icon { get; }

        /// <summary>
        /// Sets the <see cref="Icon"/> based on a private and fork state.
        /// </summary>
        /// <param name="isPrivate">Whether the repository is a private repository.</param>
        /// <param name="isFork">Whether the repository is a fork.</param>
        void SetIcon(bool isPrivate, bool isFork);
    }
}
