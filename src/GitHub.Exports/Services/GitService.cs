using System;
using System.ComponentModel.Composition;
using System.Linq;
using GitHub.Primitives;
using LibGit2Sharp;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

namespace GitHub.Services
{
    [Export(typeof(IVSServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GitService : IGitService
    {
        /// <summary>
        /// Returns the URL of the remote named "origin" for the specified <see cref="repository"/>. If the repository
        /// is null or no remote named origin exists, this method returns null
        /// </summary>
        /// <param name="repository">The repository to look at for the remote.</param>
        /// <returns>A <see cref="UriString"/> representing the origin or null if none found.</returns>
        public UriString GetUri(IRepository repository)
        {
            return UriString.ToUriString(GetUriFromRepository(repository)?.ToRepositoryUrl());
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="UriString"/> for the repository's
        /// remote named "origin" if one is found
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>A <see cref="UriString"/> representing the origin or null if none found.</returns>
        public UriString GetUri(string path)
        {
            return GetUri(GetRepo(path));
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="UriString"/> for the repository's
        /// remote named "origin" if one is found
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the path specified by the RepositoryPath property of the specified
        /// <see cref="repoInfo"/> is a repository. If it's not, it then walks up the parent directories until it
        /// either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="repoInfo">The repository information containing the path to start probing</param>
        /// <returns>A <see cref="UriString"/> representing the origin or null if none found.</returns>
        public UriString GetUri(IGitRepositoryInfo repoInfo)
        {
            return GetUri(GetRepo(repoInfo));
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="IRepository"/> instance for the
        /// repository.
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the path specified by the RepositoryPath property of the specified
        /// <see cref="repoInfo"/> is a repository. If it's not, it then walks up the parent directories until it
        /// either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="repoInfo">The repository information containing the path to start probing</param>
        /// <returns>An instance of <see cref="IRepository"/> or null</returns>

        public IRepository GetRepo(IGitRepositoryInfo repoInfo)
        {
            return GetRepo(repoInfo?.RepositoryPath);
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="IRepository"/> instance for the
        /// repository.
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>An instance of <see cref="IRepository"/> or null</returns>
        public IRepository GetRepo(string path)
        {
            var repoPath = Repository.Discover(path);
            return repoPath == null ? null : new Repository(repoPath);
        }

        internal static UriString GetUriFromRepository(IRepository repo)
        {
            return repo
                ?.Network
                .Remotes
                .FirstOrDefault(x => x.Name.Equals("origin", StringComparison.Ordinal))
                ?.Url;
        }
    }
}
