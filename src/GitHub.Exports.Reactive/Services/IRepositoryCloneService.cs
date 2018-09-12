using System;
using System.Collections.Generic;
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
        /// Default path to clone things to, used as fallback if we can't find the correct path
        /// from VS.
        /// </summary>
        string DefaultClonePath { get; }

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
        /// <returns></returns>
        Task CloneRepository(
            string cloneUrl,
            string repositoryPath,
            object progress = null);

        /// <summary>
        /// Clones the specificed repository into the specified directory or opens it if the repository already exists.
        /// </summary>
        /// <param name="cloneUrl">The url of the repository to clone.</param>
        /// <param name="repositoryPath">The repository directory.</param>
        /// <param name="progress">
        /// An object through which to report progress. This must be of type
        /// System.IProgress&lt;Microsoft.VisualStudio.Shell.ServiceProgressData&gt;, but
        /// as that type is only available in VS2017+ it is typed as <see cref="object"/> here.
        /// </param>
        /// <returns></returns>
        Task CloneOrOpenRepository(
            string cloneUrl,
            string repositoryPath,
            object progress = null);

        /// <summary>
        /// Checks whether the specified destination path already exists.
        /// </summary>
        /// <param name="path">The destination path.</param>
        /// <returns>
        /// true if a file or directory is already present at <paramref name="path"/>; otherwise false.
        /// </returns>
        bool DestinationExists(string path);

        Task<ViewerRepositoriesModel> ReadViewerRepositories(HostAddress address);
    }
}
