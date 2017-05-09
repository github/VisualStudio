using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;

namespace GitHub.InlineReviews.Services
{
    [Export(typeof(IPullRequestReviewSessionManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestReviewSessionManager : IPullRequestReviewSessionManager, IDisposable
    {
        readonly IPullRequestService service;
        readonly IRepositoryHosts hosts;
        readonly ITeamExplorerServiceHolder teamExplorerService;
        readonly BehaviorSubject<IPullRequestReviewSession> currentSession = new BehaviorSubject<IPullRequestReviewSession>(null);
        ILocalRepositoryModel repository;
        PullRequestReviewSession session;

        [ImportingConstructor]
        public PullRequestReviewSessionManager(
            IPullRequestService service,
            IRepositoryHosts hosts,
            ITeamExplorerServiceHolder teamExplorerService)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            Guard.ArgumentNotNull(teamExplorerService, nameof(teamExplorerService));

            this.service = service;
            this.hosts = hosts;
            this.teamExplorerService = teamExplorerService;
            teamExplorerService.Subscribe(this, RepoChanged);
        }

        public IObservable<IPullRequestReviewSession> CurrentSession => currentSession;

        public void Dispose()
        {
            currentSession.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<IPullRequestReviewSession> GetSession(IPullRequestModel pullRequest)
        {
            if (pullRequest.Number == session?.PullRequest.Number)
            {
                return session;
            }
            else
            {
                var modelService = hosts.LookupHost(HostAddress.Create(repository.CloneUrl))?.ModelService;

                return new PullRequestReviewSession(
                    await modelService.GetCurrentUser(),
                    pullRequest,
                    repository);
            }
        }


        async void RepoChanged(ILocalRepositoryModel repository)
        {
            PullRequestReviewSession session = null;

            this.repository = repository;

            if (repository != null)
            {
                var modelService = hosts.LookupHost(HostAddress.Create(repository.CloneUrl))?.ModelService;

                if (modelService != null)
                {
                    var pullRequest = await GetPullRequestForTip(modelService, repository);

                    if (pullRequest != null)
                    {
                        session = new PullRequestReviewSession(
                            await modelService.GetCurrentUser(),
                            pullRequest,
                            repository);
                    }
                }
            }

            this.session = session;
            currentSession.OnNext(this.session);
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
