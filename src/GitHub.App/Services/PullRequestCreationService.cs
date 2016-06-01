using System;
using System.ComponentModel.Composition;
using System.Reactive;
using GitHub.Api;
using GitHub.Extensions.Reactive;
using Octokit;
using GitHub.Models;

namespace GitHub.Services
{
    [Export(typeof(IPullRequestCreationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class PullRequestCreationService : IPullRequestCreationService
    {
        public IObservable<Unit> CreatePullRequest(NewPullRequest newPullRequest, ISimpleRepositoryModel repository, IApiClient apiClient)
        {
            return apiClient.CreatePullRequest(newPullRequest,repository.CloneUrl.Owner, repository.CloneUrl.RepositoryName)
                   .SelectUnit();
        }
    }
}
