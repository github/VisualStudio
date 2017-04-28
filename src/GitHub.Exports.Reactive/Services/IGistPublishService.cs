using System;
using Octokit;
using GitHub.Api;

namespace GitHub.Services
{
    public interface IGistPublishService
    {
        /// <summary>
        /// Publishes a gist to GitHub.
        /// </summary>
        /// <param name="apiClient">The client to use to post to GitHub.</param>
        /// <param name="gist">The new gist to post.</param>
        /// <returns>The created gist.</returns>
        IObservable<Gist> PublishGist(IApiClient apiClient, NewGist gist);
    }
}
