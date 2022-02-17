using System.Threading;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    /// <summary>
    /// Service used to clone GitHub repositories.
    /// </summary>
    public interface IRepositoryCloneService
    {
        /// <summary>
        /// Default path to clone things to
        /// </summary>
        string DefaultClonePath { get; }

        /// <summary>
        /// Default layout of repository directories.
        /// </summary>
        RepositoryLayout DefaultRepositoryLayout { get; }

        /// <summary>
        /// Infer the default clone path and layout from an example repository path and clone URL.
        /// </summary>
        void SetDefaultClonePath(string repositoryPath, UriString cloneUrl);

        /// <summary>
        /// Clones the specificed repository into the specified directory.
        /// </summary>
        /// <param name="cloneUrl">The url of the repository to clone.</param>
        /// <param name="repositoryPath">The directory that will contain the repository directory.</param>
        /// <param name="progress">
        /// An object through which to report progress. This must be of type
        /// System.IProgress&lt;Microsoft.VisualStudio.Shell.ServiceProgressData&gt;, but
        /// as that type is only available in VS2017+ it is typed as <see cref="object"/> here.
        /// </param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        Task CloneRepository(
            string cloneUrl,
            string repositoryPath,
            object progress = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// Clones the specified repository into the specified directory or opens it if the directory already exists.
        /// </summary>
        /// <param name="cloneDialogResult">The URL and path of the repository to clone or open.</param>
        /// <param name="progress">
        /// An object through which to report progress. This must be of type
        /// System.IProgress&lt;Microsoft.VisualStudio.Shell.ServiceProgressData&gt;, but
        /// as that type is only available in VS2017+ it is typed as <see cref="object"/> here.
        /// </param>
        /// <returns></returns>
        Task CloneOrOpenRepository(
            CloneDialogResult cloneDialogResult,
            object progress = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// Checks whether the specified destination directory already exists.
        /// </summary>
        /// <param name="path">The destination path.</param>
        /// <returns>
        /// true if a directory is already present at <paramref name="path"/>; otherwise false.
        /// </returns>
        bool DestinationDirectoryExists(string path);

        /// <summary>
        /// Checks whether the specified destination directory is empty.
        /// </summary>
        /// <param name="path">The destination path.</param>
        /// <returns>
        /// true if a directory is empty <paramref name="path"/>; otherwise false.
        /// </returns>
        bool DestinationDirectoryEmpty(string path);

        /// <summary>
        /// Checks whether the specified destination file already exists.
        /// </summary>
        /// <param name="path">The destination file.</param>
        /// <returns>
        /// true if a file is already present at <paramref name="path"/>; otherwise false.
        /// </returns>
        bool DestinationFileExists(string path);

        Task<ViewerRepositoriesModel> ReadViewerRepositories(HostAddress address, bool refresh = false);
    }
}
