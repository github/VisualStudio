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
        /// Clones the specificed repository into the specified directory.
        /// </summary>
        /// <param name="cloneUrl">The url of the repository to clone.</param>
        /// <param name="repositoryName">The name of the repository to clone.</param>
        /// <param name="repositoryParentDirectory">The directory that will contain the repository directory.</param>
        /// <returns></returns>
        IObservable<Unit> CloneRepository(string cloneUrl, string repositoryName, string repositoryPath);
    }
}
