using System;
using System.Diagnostics;
using System.IO;
using GitHub.Extensions;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;

namespace GitHub.Models
{
    /// <summary>
    /// The base class for local and remote repository models.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RepositoryModel : NotificationAwareObject, IRepositoryModel
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryModel"/> class.
        /// </summary>
        /// <param name="path">
        /// The path to the local repository from which repository name and clone URL will be
        /// extracted.
        /// </param>
        protected RepositoryModel(string path)
        {
            Guard.ArgumentNotNull(path, nameof(path));

            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new ArgumentException("Path does not exist", nameof(path));
            var uri = GitService.GitServiceHelper.GetUri(path);
            Name = uri?.RepositoryName ?? dir.Name;
            CloneUrl = GitService.GitServiceHelper.GetUri(path);
        }

        /// <summary>
        /// Gets the name of the repository.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the repository clone URL.
        /// </summary>
        public UriString CloneUrl
        {
            get { return cloneUrl; }
            protected set
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
            protected set
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
