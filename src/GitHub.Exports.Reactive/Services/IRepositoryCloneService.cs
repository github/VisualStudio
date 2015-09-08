using System;
using System.Reactive;

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
        /// <returns></returns>
        IObservable<Unit> CloneRepository(string cloneUrl, string repositoryName, string repositoryPath);
    }
}
