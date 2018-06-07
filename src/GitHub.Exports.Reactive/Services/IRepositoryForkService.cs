using System;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using Octokit;

namespace GitHub.Services
{
    public interface IRepositoryForkService
    {
        IObservable<Repository> ForkRepository(IApiClient apiClient, IRepositoryModel sourceRepository, NewRepositoryFork repositoryFork, bool updateOrigin, bool addUpstream, bool trackMasterUpstream);
        IObservable<object> SwitchRemotes(IRepositoryModel destinationRepository, bool updateOrigin, bool addUpstream, bool trackMasterUpstream);
    }
}
