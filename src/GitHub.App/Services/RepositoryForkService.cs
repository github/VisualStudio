using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Logging;
using GitHub.Models;
using Octokit;
using ReactiveUI;
using Serilog;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryForkService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryForkService : IRepositoryForkService
    {
        static readonly ILogger log = LogManager.ForContext<RepositoryForkService>();

        readonly IGitClient gitClient;
        readonly IVSGitServices vsGitServices;

        [ImportingConstructor]
        public RepositoryForkService(IGitClient gitClient, IVSGitServices vsGitServices)
        {
            this.gitClient = gitClient;
            this.vsGitServices = vsGitServices;
        }

        public IObservable<Repository> ForkRepository(IApiClient apiClient, IRepositoryModel sourceRepository, NewRepositoryFork repositoryFork, bool resetMasterTracking, bool addUpstream, bool updateOrigin)
        {
            return Observable.Defer(() => apiClient.ForkRepository(sourceRepository.Owner, sourceRepository.Name, repositoryFork)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(remoteRepo => new { RemoteRepo = remoteRepo, LocalRepo = vsGitServices.GetActiveRepo() }))
                .SelectMany(async repo =>
                {
                    using (repo.LocalRepo)
                    {
                        if (updateOrigin)
                        {
                            await gitClient.SetRemote(repo.LocalRepo, "origin", new Uri(repo.RemoteRepo.CloneUrl));
                        }

                        if (addUpstream)
                        {
                            await gitClient.SetRemote(repo.LocalRepo, "upstream", sourceRepository.CloneUrl.ToUri());

                            if (resetMasterTracking)
                            {
                                await gitClient.SetTrackingBranch(repo.LocalRepo, "master", "upstream");
                            }
                        }
                    }

                    return repo.RemoteRepo;
                });
        }
    }
}