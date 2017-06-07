using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using ReactiveUI;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Manages pull request sessions.
    /// </summary>
    [Export(typeof(IPullRequestSessionManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestSessionManager : ReactiveObject, IPullRequestSessionManager
    {
        readonly IPullRequestService service;
        readonly IPullRequestSessionService sessionService;
        readonly IRepositoryHosts hosts;
        readonly ITeamExplorerServiceHolder teamExplorerService;
        IPullRequestSession currentSession;
        ILocalRepositoryModel repository;

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
        public IPullRequestSession CurrentSession
        {
            get { return currentSession; }
            private set { this.RaiseAndSetIfChanged(ref currentSession, value); }
        }

        /// <inheritdoc/>
        public async Task<IPullRequestSession> GetSession(IPullRequestModel pullRequest)
        {
            if (pullRequest.Number == CurrentSession?.PullRequest.Number)
            {
                await CurrentSession.Update(pullRequest);
                return CurrentSession;
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

        /// <inheritdoc/>
        public PullRequestTextBufferInfo GetTextBufferInfo(ITextBuffer buffer)
        {
            var result = buffer.Properties.GetProperty<PullRequestTextBufferInfo>(typeof(PullRequestTextBufferInfo), null);

            if (result == null && CurrentSession != null)
            {
                var document = buffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));

                if (document != null)
                {
                    result = new PullRequestTextBufferInfo(
                        CurrentSession,
                        CurrentSession.GetRelativePath(document.FilePath),
                        false);
                }
            }

            return result;
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

                CurrentSession = session;
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
