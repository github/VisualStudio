using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Models;
using LibGit2Sharp;
using Octokit;

namespace GitHub.Services
{
    [Export(typeof(IGistPublishService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GistPublishService : IGistPublishService
    {
        /// <summary>
        /// Publishes a gist to GitHub.
        /// </summary>
        /// <param name="apiClient">The client to use to post to GitHub.</param>
        /// <param name="gist">The new gist to post.</param>
        /// <returns>The created gist.</returns>
        public IObservable<Gist> PublishGist(IApiClient apiClient, NewGist gist)
        {
            return Observable.Defer(() => apiClient.CreateGist(gist));
        }
    }
}
