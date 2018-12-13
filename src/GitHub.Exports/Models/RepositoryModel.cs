using System;
using System.Diagnostics;
using GitHub.Extensions;
using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.Models
{
    /// <summary>
    /// The base class for local and remote repository models.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryModel : NotificationAwareObject
    {
        UriString cloneUrl;
        Octicon icon;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryModel"/> class.
        /// </summary>
        /// <param name="name">The repository name.</param>
        /// <param name="cloneUrl">The repository's clone URL.</param>
        public RepositoryModel(
            string name,
            UriString cloneUrl)
        {
            Guard.ArgumentNotEmptyString(name, nameof(name));
            Guard.ArgumentNotNull(cloneUrl, nameof(cloneUrl));

            Name = name;
            CloneUrl = cloneUrl;
        }

        protected RepositoryModel()
        {
        }

        /// <summary>
        /// Gets the name of the repository.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the repository clone URL.
        /// </summary>
        public UriString CloneUrl
        {
            get { return cloneUrl; }
            set
            {
                if (cloneUrl != value)
                {
                    cloneUrl = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the name of the owner of the repository, taken from the clone URL.
        /// </summary>
        public string Owner => CloneUrl?.Owner ?? string.Empty;

        /// <summary>
        /// Gets an icon for the repository that displays its private and fork state.
        /// </summary>
        public Octicon Icon
        {
            get { return icon; }
            set
            {
                if (icon != value)
                {
                    icon = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="Icon"/> based on a private and fork state.
        /// </summary>
        /// <param name="isPrivate">Whether the repository is a private repository.</param>
        /// <param name="isFork">Whether the repository is a fork.</param>
        public void SetIcon(bool isPrivate, bool isFork)
        {
            Icon = isPrivate
                ? Octicon.@lock
                : isFork
                    ? Octicon.repo_forked
                    : Octicon.repo;
        }
    }
}
