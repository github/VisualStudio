using System;
using System.ComponentModel.Composition;
using GitHub.Models;

namespace GitHub.Services
{
    [Export(typeof(IPullRequestService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestService : IPullRequestService
    {
        public IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host, ISimpleRepositoryModel repository, IPullRequestModel pullRequest)
        {
            return host.ModelService.CreatePullRequest(pullRequest, repository);
        }
    }
}