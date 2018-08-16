using System;
using System.Reactive;
using GitHub.Api;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Service used to create a repository both on GitHub and locally and match the two up.
    /// </summary>
    public interface IRepositoryCreationService
    {
        /// <summary>
        /// Creates a repository on GitHub and locally and matches the two up.
        /// </summary>
        /// <remarks>
        /// An implementation of this interface might create the repository on GitHub and then clone it locally. 
        /// Alternatively, it could create it locally and publish it to GitHub.
        /// </remarks>
        /// <param name="newRepository">The repository to create.</param>
        /// <param name="account">The owner of the repository to create.</param>
        /// <param name="directory">
        /// The directory in which to create the repository. The repository folder (named after the repository) will
        /// be created in this directory.
        /// </param>
        /// <param name="apiClient">The API client associated with the host where this repository is created.</param>
        /// <returns></returns>
        IObservable<ILocalRepositoryModel> CreateRepository(
            Octokit.NewRepository newRepository,
            IAccount account,
            string directory,
            IApiClient apiClient);

        /// <summary>
        /// Default path to clone things to, used as fallback if we can't find the correct path
        /// from VS.
        /// </summary>
        string DefaultClonePath { get; }
    }
}
