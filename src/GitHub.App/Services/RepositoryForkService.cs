using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using LibGit2Sharp;
using Octokit;
using ReactiveUI;
using Serilog;
using Repository = Octokit.Repository;

namespace GitHub.Services
{
    [Export(typeof(IRepositoryForkService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RepositoryForkService : IRepositoryForkService
    {
        readonly IGitClient gitClient;
        readonly IVSGitServices vsGitServices;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public RepositoryForkService(IGitClient gitClient, IVSGitServices vsGitServices, IUsageTracker usageTracker)
        {
            this.gitClient = gitClient;
            this.vsGitServices = vsGitServices;
            this.usageTracker = usageTracker;
        }

        public IObservable<Repository> ForkRepository(IApiClient apiClient, IRepositoryModel sourceRepository, NewRepositoryFork repositoryFork, bool updateOrigin, bool addUpstream, bool trackMasterUpstream)
        {
            return Observable.Defer(() => apiClient.ForkRepository(sourceRepository.Owner, sourceRepository.Name, repositoryFork)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(remoteRepo => new { RemoteRepo = remoteRepo, ActiveRepo = vsGitServices.GetActiveRepo() }))
                .SelectMany(async repo =>
                {
                    using (repo.ActiveRepo)
                    {
                        var originUri = repo.RemoteRepo != null ? new Uri(repo.RemoteRepo.CloneUrl) : null;
                        var upstreamUri = addUpstream ? sourceRepository.CloneUrl.ToUri() : null;

                        await SwitchRemotes(repo.ActiveRepo, originUri, upstreamUri, trackMasterUpstream);

                        RecordForkRepositoryUsage(updateOrigin, addUpstream, trackMasterUpstream).Forget();

                        return repo.RemoteRepo;
                    }
                });
        }

        private async Task RecordForkRepositoryUsage(bool updateOrigin, bool addUpstream, bool trackMasterUpstream)
        {
            await usageTracker.IncrementCounter(model => model.NumberOfReposForked);

            if (updateOrigin)
            {
                await usageTracker.IncrementCounter(model => model.NumberOfOriginsUpdatedWhenForkingRepo);
            }

            if (addUpstream)
            {
                await usageTracker.IncrementCounter(model => model.NumberOfUpstreamsAddedWhenForkingRepo);
            }

            if (trackMasterUpstream)
            {
                await usageTracker.IncrementCounter(model => model.NumberOfTrackMasterUpstreamWhenForkingRepo);
            }
        }

        public IObservable<object> SwitchRemotes(IRepositoryModel destinationRepository, bool updateOrigin, bool addUpstream, bool trackMasterUpstream)
        {
            return Observable.Defer(() => Observable.Return(new object())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => vsGitServices.GetActiveRepo()))
                .SelectMany(async activeRepo =>
                {
                    using (activeRepo)
                    {
                        Uri currentOrigin = null;

                        if (addUpstream)
                        {
                            var remote = await gitClient.GetHttpRemote(activeRepo, "origin");
                            currentOrigin = new Uri(remote.Url);
                        }

                        await SwitchRemotes(activeRepo, updateOrigin ? destinationRepository.CloneUrl.ToUri() : null,
                            currentOrigin, trackMasterUpstream);

                        return new object();
                    }
                });
        }

        private async Task SwitchRemotes(IRepository repository, Uri originUri = null, Uri upstreamUri = null, bool trackMasterUpstream = false)
        {
            if (originUri != null || upstreamUri != null)
            {
                if (originUri != null)
                {
                    await gitClient.SetRemote(repository, "origin", originUri);
                }

                if (upstreamUri != null)
                {
                    await gitClient.SetRemote(repository, "upstream", upstreamUri);

                    await gitClient.Fetch(repository, "upstream");

                    if (trackMasterUpstream)
                    {
                        await gitClient.SetTrackingBranch(repository, "master", "upstream");
                    }
                }
            }
        }
    }
}