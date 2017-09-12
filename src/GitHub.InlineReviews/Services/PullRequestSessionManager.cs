using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using ReactiveUI;
using Serilog;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Manages pull request sessions.
    /// </summary>
    [Export(typeof(IPullRequestSessionManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestSessionManager : ReactiveObject, IPullRequestSessionManager
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestSessionManager>();
        readonly IPullRequestService service;
        readonly IPullRequestSessionService sessionService;
        readonly IRepositoryHosts hosts;
        readonly ITeamExplorerServiceHolder teamExplorerService;
        readonly Dictionary<Tuple<string, int>, WeakReference<PullRequestSession>> sessions =
            new Dictionary<Tuple<string, int>, WeakReference<PullRequestSession>>();
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
            teamExplorerService.Subscribe(this, x => RepoChanged(x).Forget());
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
            if (await service.EnsureLocalBranchesAreMarkedAsPullRequests(repository, pullRequest))
            {
                // The branch for the PR was not previously marked with the PR number in the git
                // config so we didn't pick up that the current branch is a PR branch. That has
                // now been corrected, so call RepoChanged to make sure everything is up-to-date.
                await RepoChanged(repository);
            }

            return await GetSessionInternal(pullRequest);
        }

        /// <inheritdoc/>
        public PullRequestTextBufferInfo GetTextBufferInfo(ITextBuffer buffer)
        {
            var projectionBuffer = buffer as IProjectionBuffer;
            PullRequestTextBufferInfo result;

            if (buffer.Properties.TryGetProperty(typeof(PullRequestTextBufferInfo), out result))
            {
                return result;
            }

            if (projectionBuffer != null)
            {
                foreach (var sourceBuffer in projectionBuffer.SourceBuffers)
                {
                    var sourceBufferInfo = GetTextBufferInfo(sourceBuffer);
                    if (sourceBufferInfo != null) return sourceBufferInfo;
                }
            }

            return null;
        }

        async Task RepoChanged(ILocalRepositoryModel repository)
        {
            try
            {
                await ThreadingHelper.SwitchToMainThreadAsync();
                await EnsureLoggedIn(repository);

                if (repository != this.repository)
                {
                    this.repository = repository;
                    CurrentSession = null;
                    sessions.Clear();
                }

                if (string.IsNullOrWhiteSpace(repository?.CloneUrl)) return;

                var modelService = hosts.LookupHost(HostAddress.Create(repository.CloneUrl))?.ModelService;
                var session = CurrentSession;

                if (modelService != null)
                {
                    var pr = await service.GetPullRequestForCurrentBranch(repository).FirstOrDefaultAsync();

                    if (pr?.Item1 != (CurrentSession?.PullRequest.Base.RepositoryCloneUrl.Owner) &&
                        pr?.Item2 != (CurrentSession?.PullRequest.Number))
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
            catch (Exception e)
            {
                log.Error(e, "Error changing repository.");
            }
        }

        async Task<IPullRequestModel> GetPullRequestForTip(IModelService modelService, ILocalRepositoryModel repository)
        {
            if (modelService != null)
            {
                var pr = await service.GetPullRequestForCurrentBranch(repository);
                if (pr != null) return await modelService.GetPullRequest(pr.Item1, repository.Name, pr.Item2).ToTask();
            }

            return null;
        }

        async Task<PullRequestSession> GetSessionInternal(IPullRequestModel pullRequest)
        {
            PullRequestSession session = null;
            WeakReference<PullRequestSession> weakSession;
            var key = Tuple.Create(pullRequest.Base.RepositoryCloneUrl.Owner, pullRequest.Number);

            if (sessions.TryGetValue(key, out weakSession))
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
                        key.Item1,
                        false);
                    sessions[key] = new WeakReference<PullRequestSession>(session);
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
            if (!hosts.IsLoggedInToAnyHost && !string.IsNullOrWhiteSpace(repository?.CloneUrl))
            {
                var hostAddress = HostAddress.Create(repository.CloneUrl);
                await hosts.LogInFromCache(hostAddress);
            }
        }
    }
}
