using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Rothko;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Manages pull request sessions.
    /// </summary>
    [Export(typeof(IPullRequestSessionManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestSessionManager : IPullRequestSessionManager, IDisposable
    {
        readonly IOperatingSystem os;
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly IPullRequestService service;
        readonly IRepositoryHosts hosts;
        readonly ITeamExplorerServiceHolder teamExplorerService;
        readonly BehaviorSubject<IPullRequestSession> currentSession = new BehaviorSubject<IPullRequestSession>(null);
        ILocalRepositoryModel repository;
        PullRequestSession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestSessionManager"/> class.
        /// </summary>
        /// <param name="gitService">The git service to use.</param>
        /// <param name="gitClient">The git client to use.</param>
        /// <param name="diffService">The diff service to use.</param>
        /// <param name="service">The pull request service to use.</param>
        /// <param name="hosts">The repository hosts.</param>
        /// <param name="teamExplorerService">The team explorer service to use.</param>
        [ImportingConstructor]
        public PullRequestSessionManager(
            IOperatingSystem os,
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            IPullRequestService service,
            IRepositoryHosts hosts,
            ITeamExplorerServiceHolder teamExplorerService)
        {
            Guard.ArgumentNotNull(os, nameof(os));
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(diffService, nameof(diffService));
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            Guard.ArgumentNotNull(teamExplorerService, nameof(teamExplorerService));

            this.os = os;
            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            this.service = service;
            this.hosts = hosts;
            this.teamExplorerService = teamExplorerService;
            teamExplorerService.Subscribe(this, RepoChanged);
        }

        /// <inheritdoc/>
        public IObservable<IPullRequestSession> CurrentSession => currentSession;

        /// <summary>
        /// Disposes of the object and terminates all subscriptions.
        /// </summary>
        public void Dispose()
        {
            currentSession.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestSession> GetSession(IPullRequestModel pullRequest)
        {
            if (pullRequest.Number == session?.PullRequest.Number)
            {
                return session;
            }
            else
            {
                var modelService = hosts.LookupHost(HostAddress.Create(repository.CloneUrl))?.ModelService;

                return new PullRequestSession(
                    os,
                    gitService,
                    gitClient,
                    diffService,
                    await modelService.GetCurrentUser(),
                    pullRequest,
                    repository,
                    false);
            }
        }

        async void RepoChanged(ILocalRepositoryModel repository)
        {
            try
            {
                PullRequestSession session = null;

                this.repository = repository;

                if (repository?.CloneUrl != null)
                {
                    var hostAddress = HostAddress.Create(repository.CloneUrl);

                    if (!hosts.IsLoggedInToAnyHost)
                    {
                        await hosts.LogInFromCache(hostAddress);
                    }

                    var modelService = hosts.LookupHost(hostAddress)?.ModelService;

                    if (modelService != null)
                    {
                        var pullRequest = await GetPullRequestForTip(modelService, repository);

                        if (pullRequest != null)
                        {
                            session = new PullRequestSession(
                                os,
                                gitService,
                                gitClient,
                                diffService,
                                await modelService.GetCurrentUser(),
                                pullRequest,
                                repository,
                                true);
                        }
                    }
                }

                this.session = session;
                currentSession.OnNext(this.session);
            }
            catch
            {
                // TODO: Log
            }
        }

        async Task<IPullRequestModel> GetPullRequestForTip(IModelService modelService, ILocalRepositoryModel repository)
        {
            var number = await service.GetPullRequestForCurrentBranch(repository);
            return number != 0 ?
                await modelService.GetPullRequest(repository, number).ToTask() :
                null;
        }
    }
}
