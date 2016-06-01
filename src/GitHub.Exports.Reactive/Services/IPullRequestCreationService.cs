using GitHub.Api;
using GitHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IPullRequestCreationService
    {
        /// <summary>
        /// Makes a new pull request
        /// </summary>
        /// <param name="newPullRequest">The pull request to create</param>
        /// <param name="repository">The repository associated with the pull request</param>
        /// <param name="apiClient">The client to use to post to GitHub.</param>
        /// <returns></returns>
        IObservable<Unit> CreatePullRequest(
                Octokit.NewPullRequest newPullRequest,
                ISimpleRepositoryModel repository,
                IApiClient apiClient);
     }
 }

