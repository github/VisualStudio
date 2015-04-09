using System;
using GitHub.Api;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IRepositoryPublishService
    {
        /// <summary>
        /// Takes a local repository and publishes it to GitHub.com.
        /// </summary>
        /// <param name="newRepository">The repository to create on GitHub.</param>
        /// <param name="account">The account to associate with the repository.</param>
        /// <param name="apiClient">The client to use to post to GitHub.</param>
        /// <returns></returns>
        IObservable<Octokit.Repository> PublishRepository(
            Octokit.NewRepository newRepository,
            IAccount account,
            IApiClient apiClient);

        /// <summary>
        /// Retrieves the repository name.
        /// </summary>
        string LocalRepositoryName { get; }
    }
}
