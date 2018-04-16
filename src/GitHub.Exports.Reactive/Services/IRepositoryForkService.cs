using System;
using GitHub.Api;
using GitHub.Models;
using Octokit;

namespace GitHub.Services
{
    public interface IRepositoryForkService
    {
        IObservable<Repository> ForkRepository(IApiClient apiClient, IRepositoryModel sourceRepository, NewRepositoryFork repositoryFork, bool resetMasterTracking, bool addUpstream, bool updateOrigin);
    }
}
