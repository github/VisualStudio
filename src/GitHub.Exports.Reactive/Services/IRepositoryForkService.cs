using System;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using Octokit;

namespace GitHub.Services
{
    public interface IRepositoryForkService
    {
        IObservable<Repository> ForkRepository(IApiClient apiClient, IRepositoryModel sourceRepository, NewRepositoryFork repositoryFork, bool updateOrigin, bool addUpstream, bool trackMasterUpstream);
        IObservable<Unit> SwitchRemotes(IRepositoryModel destinationRepository, bool updateOrigin, bool addUpstream, bool trackMasterUpstream);
    }
}
