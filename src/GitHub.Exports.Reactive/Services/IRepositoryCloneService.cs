using System;
using System.Reactive;
using System.Threading.Tasks;

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
        /// <param name="repositoryName">The name of the repository to clone.</param>
        /// <param name="repositoryPath">The directory that will contain the repository directory.</param>
        /// <param name="progress">
        /// An object through which to report progress. This must be of type
        /// System.IProgress&lt;Microsoft.VisualStudio.Shell.ServiceProgressData&gt;, but
        /// as that type is only available in VS2017+ it is typed as <see cref="object"/> here.
        /// </param>
        /// <returns></returns>
        Task CloneRepository(
            string cloneUrl,
            string repositoryName,
            string repositoryPath,
            object progress = null);
    }
}
