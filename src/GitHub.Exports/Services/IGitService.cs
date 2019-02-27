using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using LibGit2Sharp;

namespace GitHub.Services
{
    public interface IGitService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRepositoryModel"/> class.
        /// </summary>
        /// <param name="localPath">The repository's local path.</param>
        /// <returns>A repository model.</returns>
        LocalRepositoryModel CreateLocalRepositoryModel(string localPath);

        /// <summary>
        /// Creates a new branch model for the current branch.
        /// </summary>
        /// <param name="model">The <see cref="LocalRepositoryModel" /> to create a current branch model for.</param>
        /// <returns>A new branch model.</returns>
        BranchModel GetBranch(LocalRepositoryModel model);

        /// <summary>
        /// Returns the URL of the remote for the specified <see cref="repository"/>. If the repository
        /// is null or no remote exists, this method returns null
        /// </summary>
        /// <param name="repository">The repository to look at for the remote.</param>
        /// <param name="remote">The remote name to look for</param>
        /// <returns>A <see cref="UriString"/> representing the origin or null if none found.</returns>
        UriString GetUri(IRepository repository, string remote = "origin");

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="UriString"/> for the repository's
        /// remote if one is found
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <param name="remote">The remote name to look for</param>
        /// <returns>A <see cref="UriString"/> representing the origin or null if none found.</returns>
        UriString GetUri(string path, string remote = "origin");

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="RepositoryModel"/> instance for the
        /// repository.
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>An instance of <see cref="RepositoryModel"/> or null</returns>
        IRepository GetRepository(string path);

        /// <summary>
        /// Returns a <see cref="UriString"/> representing the uri of a remote.
        /// </summary>
        /// <param name="repo">The repository to look at for the remote.</param>
        /// <param name="remote">The remote name to look for</param>
        /// <returns></returns>
        UriString GetRemoteUri(IRepository repo, string remote = "origin");

        /// <summary>
        /// Finds the latest pushed commit of a file and returns the sha of that commit. Returns null when no commits have 
        /// been found in any remote branches or the current local branch. 
        /// </summary>
        /// <param name="path">The local path of a repository or a file inside a repository. This cannot be null.</param>
        /// <param name="remote">The remote name to look for</param>
        /// <returns></returns>
        Task<string> GetLatestPushedSha(string path, string remote = "origin");
    }
}