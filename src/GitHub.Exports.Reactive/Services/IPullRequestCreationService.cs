using GitHub.Api;
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
            /// Takes a local repository and publishes it to GitHub.com.
            /// </summary>
            /// <param name="newRepository">The repository to create on GitHub.</param>
            /// <param name="account">The account to associate with the repository.</param>
            /// <param name="apiClient">The client to use to post to GitHub.</param>
            /// <returns></returns>
            IObservable<Unit> CreatePullRequest(
                Octokit.NewPullRequest newPullRequest,
                IApiClient apiClient);

            /// <summary>
            /// Retrieves the repository name.
            /// </summary>
            string LocalRepositoryName { get; }
        }
 }

