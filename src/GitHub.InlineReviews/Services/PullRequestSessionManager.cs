using System;
using System.Collections.Generic;
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
        readonly Dictionary<int, WeakReference<PullRequestSession>> sessions =
            new Dictionary<int, WeakReference<PullRequestSession>>();
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
            return await GetSessionInternal(pullRequest);
        }

        /// <inheritdoc/>
        public PullRequestTextBufferInfo GetTextBufferInfo(ITextBuffer buffer)
        {
            return buffer.Properties.GetProperty<PullRequestTextBufferInfo>(typeof(PullRequestTextBufferInfo), null);
        }

        async void RepoChanged(ILocalRepositoryModel repository)
        {
            try
            {
                await EnsureLoggedIn(repository);

                if (repository != this.repository)
                {
                    this.repository = repository;
                    CurrentSession = null;
                    sessions.Clear();
                }

                var modelService = hosts.LookupHost(HostAddress.Create(repository.CloneUrl))?.ModelService;
                var session = CurrentSession;

                if (modelService != null)
                {
                    var number = await service.GetPullRequestForCurrentBranch(repository);

                    if (number != (CurrentSession?.PullRequest.Number ?? 0))
                    {
                        var pullRequest = await GetPullRequestForTip(modelService, repository);

                        if (pullRequest != null)
                        {
                            var newSession = await GetSessionInternal(pullRequest); ;
                            if (newSession != null) newSession.IsCheckedOut = true;
                            session = newSession;
                        }
                    }
                }
                else
                {
                    session = null;
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
            if (modelService != null)
            {
                var number = await service.GetPullRequestForCurrentBranch(repository);
                if (number != 0) return await modelService.GetPullRequest(repository, number).ToTask();
            }

            return null;
        }

        async Task<PullRequestSession> GetSessionInternal(IPullRequestModel pullRequest)
        {
            PullRequestSession session = null;
            WeakReference<PullRequestSession> weakSession;

            if (sessions.TryGetValue(pullRequest.Number, out weakSession))
            {
                weakSession.TryGetTarget(out session);
            }

            if (session == null)
            {
                var modelService = hosts.LookupHost(HostAddress.Create(repository.CloneUrl))?.ModelService;

                if (modelService != null)
                {
                    session = new PullRequestSession(
                        sessionService,
                        await modelService.GetCurrentUser(),
                        pullRequest,
                        repository,
                        false);
                    sessions[pullRequest.Number] = new WeakReference<PullRequestSession>(session);
                }
            }
            else
            {
                await session.Update(pullRequest);
            }

            return session;
        }

        async Task EnsureLoggedIn(ILocalRepositoryModel repository)
        {
            if (!hosts.IsLoggedInToAnyHost && repository != null)
            {
                var hostAddress = HostAddress.Create(repository.CloneUrl);
                await hosts.LogInFromCache(hostAddress);
            }
        }
    }
}
