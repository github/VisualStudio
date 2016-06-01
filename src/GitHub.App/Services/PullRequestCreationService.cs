using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions.Reactive;
using GitHub.Models;
using Octokit;
using GitHub.Services;

namespace GitHub.Services
{
    [Export(typeof(IPullRequestCreationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class PullRequestCreationService : IPullRequestCreationService
    {
        public string LocalRepositoryName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IObservable<Unit> CreatePullRequest(NewPullRequest newPullRequest, IApiClient apiClient)
        {
            return apiClient.CreatePullRequest(newPullRequest,"","")
                   .SelectUnit();
        }
    }
}
