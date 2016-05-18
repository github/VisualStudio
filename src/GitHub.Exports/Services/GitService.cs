using System.ComponentModel.Composition;
using GitHub.Primitives;
using LibGit2Sharp;

namespace GitHub.Services
{
    [Export(typeof(IGitService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitService : IGitService
    {
        /// <summary>
        /// Returns the URL of the remote named "origin" for the specified <see cref="repository"/>. If the repository
        /// is null or no remote named origin exists, this method returns null
        /// </summary>
        /// <param name="repository">The repository to look at for the remote.</param>
        /// <returns>Returns a <see cref="UriString"/> representing the uri of the "origin" remote normalized to a GitHub repository url or null if none found.</returns>
        public UriString GetUri(IRepository repository)
        {
            return UriString.ToUriString(GetOriginUri(repository)?.ToRepositoryUrl());
        }

        /// <summary>
        /// Returns a <see cref="UriString"/> representing the uri of the "origin" remote normalized to a GitHub repository url or null if none found.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns>Returns a <see cref="UriString"/> representing the uri of the "origin" remote normalized to a GitHub repository url or null if none found.</returns>
        public static UriString GetGitHubUri(IRepository repository)
        {
            return GitServiceHelper.GetUri(repository);
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a normalized GitHub uri <see cref="UriString"/>
        /// for the repository's remote named "origin" if one is found
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>Returns a <see cref="UriString"/> representing the uri of the "origin" remote normalized to a GitHub repository url or null if none found.</returns>
        public UriString GetUri(string path)
        {
            return GetUri(GetRepo(path));
        }

        /// <summary>
        /// Probes for a git repository and if one is found, returns a normalized GitHub uri <see cref="UriString"/>
        /// for the repository's remote named "origin" if one is found
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>Returns a <see cref="UriString"/> representing the uri of the "origin" remote normalized to a GitHub repository url or null if none found.</returns>
        public static UriString GetUriFromPath(string path)
        {
            return GitServiceHelper.GetUri(path);
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
        public static IRepository GetRepoFromPath(string path)
        {
            return GitServiceHelper.GetRepo(path);
        }

        /// <summary>
        /// Returns a <see cref="UriString"/> representing the uri of the "origin" remote with no modifications.
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        public static UriString GetOriginUri(IRepository repo)
        {
            return repo
                ?.Network
                .Remotes["origin"]
                ?.Url;
        }

        public static IGitService GitServiceHelper => VisualStudio.Services.DefaultExportProvider.GetExportedValueOrDefault<IGitService>() ?? new GitService();
    }
}
