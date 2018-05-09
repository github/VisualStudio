using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.ViewModels.Dialog;
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
        static readonly ILogger log = LogManager.ForContext<RepositoryForkService>();

        readonly IGitClient gitClient;
        readonly IVSGitServices vsGitServices;
        readonly IVSGitExt vsGitExt;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public RepositoryForkService(IGitClient gitClient, IVSGitServices vsGitServices, IVSGitExt vsGitExt, IUsageTracker usageTracker)
        {
            this.gitClient = gitClient;
            this.vsGitServices = vsGitServices;
            this.vsGitExt = vsGitExt;
            this.usageTracker = usageTracker;
        }

        public IObservable<Repository> ForkRepository(IApiClient apiClient, IRepositoryModel sourceRepository, NewRepositoryFork repositoryFork, bool updateOrigin, bool addUpstream, bool trackMasterUpstream)
        {
            log.Verbose("ForkRepository Source:{SourceOwner}/{SourceName} To:{DestinationOwner}", sourceRepository.Owner, sourceRepository.Name, repositoryFork.Organization ?? "[Current User]");
            log.Verbose("ForkRepository updateOrigin:{UpdateOrigin} addUpstream:{AddUpstream} trackMasterUpstream:{TrackMasterUpstream}", updateOrigin, addUpstream, trackMasterUpstream);

            RecordForkRepositoryUsage(updateOrigin, addUpstream, trackMasterUpstream).Forget();

            return Observable.Defer(() => apiClient.ForkRepository(sourceRepository.Owner, sourceRepository.Name, repositoryFork)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(remoteRepo => new { RemoteRepo = remoteRepo, ActiveRepo = updateOrigin ? vsGitServices.GetActiveRepo() : null }))
                .SelectMany(async repo =>
                {
                    if (repo.ActiveRepo != null)
                    {
                        using (repo.ActiveRepo)
                        {
                            var originUri = repo.RemoteRepo != null ? new Uri(repo.RemoteRepo.CloneUrl) : null;
                            var upstreamUri = addUpstream ? sourceRepository.CloneUrl.ToUri() : null;

                            await SwitchRemotes(repo.ActiveRepo, originUri, upstreamUri, trackMasterUpstream);
                        }
                    }

                    return repo.RemoteRepo;
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
                    }

                    if (updateOrigin)
                    {
                        vsGitExt.RefreshActiveRepositories();

                        var updatedRepository = vsGitExt.ActiveRepositories.FirstOrDefault();
                        log.Assert(updatedRepository.CloneUrl == destinationRepository.CloneUrl,
                            "CloneUrl is {UpdatedRepository} not {DestinationRepository}", updatedRepository.CloneUrl, destinationRepository.CloneUrl);
                    }

                    return new object();
                });
        }

        private async Task SwitchRemotes(IRepository repository, Uri originUri, Uri upstreamUri = null, bool trackMasterUpstream = false)
        {
            Guard.ArgumentNotNull(originUri, nameof(originUri));

            log.Verbose("Set remote origin to {OriginUri}", originUri);

            await gitClient.SetRemote(repository, "origin", originUri);

            if (upstreamUri != null)
            {
                log.Verbose("Set remote upstream to {UpstreamUri}", upstreamUri);

                await gitClient.SetRemote(repository, "upstream", upstreamUri);

                await gitClient.Fetch(repository, "upstream");

                if (trackMasterUpstream)
                {
                    log.Verbose("set master tracking to upstream");

                    await gitClient.SetTrackingBranch(repository, "master", "upstream");
                }
            }
        }
    }
}