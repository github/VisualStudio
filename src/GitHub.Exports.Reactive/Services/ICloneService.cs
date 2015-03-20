using System;
using System.Reactive;

namespace GitHub.Services
{
    /// <summary>
    /// Service used to clone GitHub repositories.
    /// </summary>
    public interface ICloneService
    {
        /// <summary>
        /// Clones the specificed repository into the specified directory.
        /// </summary>
        /// <param name="repository">The repository to clone.</param>
        /// <param name="repositoryParentDirectory">The directory that will contain the repository directory.</param>
        /// <returns></returns>
        IObservable<Unit> CloneRepository(Octokit.Repository repository, string repositoryParentDirectory);
    }
}
