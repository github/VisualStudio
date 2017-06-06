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
        readonly IPullRequestService service;
        readonly IPullRequestSessionService sessionService;
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
            IPullRequestService service,
            IPullRequestSessionService sessionService,
            IRepositoryHosts hosts,
            ITeamExplorerServiceHolder teamExplorerService)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(sessionService, nameof(sessionService));
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            Guard.ArgumentNotNull(teamExplorerService, nameof(teamExplorerService));

            this.service = service;
            this.sessionService = sessionService;
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
                await session.Update(pullRequest);
                return session;
            }
            else
            {
                var modelService = hosts.LookupHost(HostAddress.Create(repository.CloneUrl))?.ModelService;

                return new PullRequestSession(
                    sessionService,
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
                                sessionService,
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
