using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Helpers;
using GitHub.InlineReviews.Models;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
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
        readonly IConnectionManager connectionManager;
        readonly IModelServiceFactory modelServiceFactory;
        readonly Dictionary<Tuple<string, int>, WeakReference<PullRequestSession>> sessions =
            new Dictionary<Tuple<string, int>, WeakReference<PullRequestSession>>();
        IPullRequestSession currentSession;
        ILocalRepositoryModel repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestSessionManager"/> class.
        /// </summary>
        /// <param name="service">The PR service to use.</param>
        /// <param name="sessionService">The PR session service to use.</param>
        /// <param name="connectionManager">The connectionManager to use.</param>
        /// <param name="modelServiceFactory">The ModelService factory.</param>
        /// <param name="teamExplorerService">The team explorer service to use.</param>
        [ImportingConstructor]
        public PullRequestSessionManager(
            IPullRequestService service,
            IPullRequestSessionService sessionService,
            IConnectionManager connectionManager,
            IModelServiceFactory modelServiceFactory,
            ITeamExplorerContext teamExplorerContext)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(sessionService, nameof(sessionService));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));
            Guard.ArgumentNotNull(teamExplorerContext, nameof(teamExplorerContext));

            this.service = service;
            this.sessionService = sessionService;
            this.connectionManager = connectionManager;
            this.modelServiceFactory = modelServiceFactory;

            RepoChanged(teamExplorerContext.ActiveRepository).Forget();
            teamExplorerContext.StatusChanged += (s, e) =>
            {
                RepoChanged(teamExplorerContext.ActiveRepository).Forget();
            };
            teamExplorerContext.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(teamExplorerContext.ActiveRepository))
                {
                    RepoChanged(teamExplorerContext.ActiveRepository).Forget();
                }
            };
        }

        /// <inheritdoc/>
        public IPullRequestSession CurrentSession
        {
            get { return currentSession; }
            private set { this.RaiseAndSetIfChanged(ref currentSession, value); }
        }

        public async Task<IPullRequestSessionFile> GetLiveFile(
            string relativePath,
            ITextView textView,
            ITextBuffer textBuffer)
        {
            PullRequestSessionLiveFile result;

            if (!textBuffer.Properties.TryGetProperty(
                typeof(IPullRequestSessionFile),
                out result))
            {
                var dispose = new CompositeDisposable();

                result = new PullRequestSessionLiveFile(
                    relativePath,
                    textBuffer,
                    sessionService.CreateRebuildSignal());

                textBuffer.Properties.AddProperty(
                    typeof(IPullRequestSessionFile),
                    result);

                await UpdateLiveFile(result, true);

                textBuffer.Changed += TextBufferChanged;
                textView.Closed += TextViewClosed;

                dispose.Add(Disposable.Create(() =>
                {
                    textView.TextBuffer.Changed -= TextBufferChanged;
                    textView.Closed -= TextViewClosed;
                }));

                dispose.Add(result.Rebuild.Subscribe(x => UpdateLiveFile(result, x).Forget()));

                dispose.Add(this.WhenAnyValue(x => x.CurrentSession)
                    .Skip(1)
                    .Subscribe(_ => UpdateLiveFile(result, true).Forget()));
                dispose.Add(this.WhenAnyObservable(x => x.CurrentSession.PullRequestChanged)
                    .Subscribe(_ => UpdateLiveFile(result, true).Forget()));

                result.ToDispose = dispose;
            }

            return result;
        }

        /// <inheritdoc/>
        public string GetRelativePath(ITextBuffer buffer)
        {
            var document = sessionService.GetDocument(buffer);
            var path = document?.FilePath;

            if (!string.IsNullOrWhiteSpace(path) && Path.IsPathRooted(path) && repository != null)
            {
                var basePath = repository.LocalPath;

                if (path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) && path.Length > basePath.Length + 1)
                {
                    return path.Substring(basePath.Length + 1);
                }
            }

            return null;
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

        async Task RepoChanged(ILocalRepositoryModel localRepositoryModel)
        {
            try
            {
                await ThreadingHelper.SwitchToMainThreadAsync();

                if (localRepositoryModel != repository)
                {
                    repository = localRepositoryModel;
                    CurrentSession = null;
                    sessions.Clear();
                }

                if (string.IsNullOrWhiteSpace(localRepositoryModel?.CloneUrl)) return;

                var modelService = await connectionManager.GetModelService(repository, modelServiceFactory);
                var session = CurrentSession;

                if (modelService != null)
                {
                    var pr = await service.GetPullRequestForCurrentBranch(localRepositoryModel).FirstOrDefaultAsync();

                    if (pr?.Item1 != (CurrentSession?.PullRequest.Base.RepositoryCloneUrl.Owner) ||
                        pr?.Item2 != (CurrentSession?.PullRequest.Number))
                    {
                        var pullRequest = await GetPullRequestForTip(modelService, localRepositoryModel);

                        if (pullRequest != null)
                        {
                            var newSession = await GetSessionInternal(pullRequest);
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
                log.Error(e, "Error changing repository");
            }
        }

        async Task<IPullRequestModel> GetPullRequestForTip(IModelService modelService, ILocalRepositoryModel localRepositoryModel)
        {
            if (modelService != null)
            {
                var pr = await service.GetPullRequestForCurrentBranch(localRepositoryModel);
                if (pr != null) return await modelService.GetPullRequest(pr.Item1, localRepositoryModel.Name, pr.Item2).ToTask();
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
                var modelService = await connectionManager.GetModelService(repository, modelServiceFactory);

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

        async Task UpdateLiveFile(PullRequestSessionLiveFile file, bool rebuildThreads)
        {
            var session = CurrentSession;

            if (session != null)
            {
                var mergeBase = await session.GetMergeBase();
                var contents = sessionService.GetContents(file.TextBuffer);
                file.BaseSha = session.PullRequest.Base.Sha;
                file.CommitSha = await CalculateCommitSha(session, file, contents);
                file.Diff = await sessionService.Diff(
                    session.LocalRepository,
                    mergeBase,
                    session.PullRequest.Head.Sha,
                    file.RelativePath,
                    contents);

                if (rebuildThreads)
                {
                    file.InlineCommentThreads = sessionService.BuildCommentThreads(
                        session.PullRequest,
                        file.RelativePath,
                        file.Diff);
                }
                else
                {
                    var changedLines = sessionService.UpdateCommentThreads(
                        file.InlineCommentThreads,
                        file.Diff);

                    if (changedLines.Count > 0)
                    {
                        file.NotifyLinesChanged(changedLines);
                    }
                }

                file.TrackingPoints = BuildTrackingPoints(
                    file.TextBuffer.CurrentSnapshot,
                    file.InlineCommentThreads);
            }
            else
            {
                file.BaseSha = null;
                file.CommitSha = null;
                file.Diff = null;
                file.InlineCommentThreads = null;
                file.TrackingPoints = null;
            }
        }

        async Task UpdateLiveFile(PullRequestSessionLiveFile file, ITextSnapshot snapshot)
        {
            if (file.TextBuffer.CurrentSnapshot == snapshot)
            {
                await UpdateLiveFile(file, false);
            }
        }

        void InvalidateLiveThreads(PullRequestSessionLiveFile file, ITextSnapshot snapshot)
        {
            if (file.TrackingPoints != null)
            {
                var linesChanged = new List<Tuple<int, DiffSide>>();

                foreach (var thread in file.InlineCommentThreads)
                {
                    ITrackingPoint trackingPoint;

                    if (file.TrackingPoints.TryGetValue(thread, out trackingPoint))
                    {
                        var position = trackingPoint.GetPosition(snapshot);
                        var lineNumber = snapshot.GetLineNumberFromPosition(position);

                        if (thread.DiffLineType != DiffChangeType.Delete && lineNumber != thread.LineNumber)
                        {
                            linesChanged.Add(Tuple.Create(lineNumber, DiffSide.Right));
                            linesChanged.Add(Tuple.Create(thread.LineNumber, DiffSide.Right));
                            thread.LineNumber = lineNumber;
                            thread.IsStale = true;
                        }
                    }
                }

                linesChanged = linesChanged
                    .Where(x => x.Item1 >= 0)
                    .Distinct()
                    .ToList();

                if (linesChanged.Count > 0)
                {
                    file.NotifyLinesChanged(linesChanged);
                }
            }
        }

        private IDictionary<IInlineCommentThreadModel, ITrackingPoint> BuildTrackingPoints(
            ITextSnapshot snapshot,
            IReadOnlyList<IInlineCommentThreadModel> threads)
        {
            var result = new Dictionary<IInlineCommentThreadModel, ITrackingPoint>();

            foreach (var thread in threads)
            {
                if (thread.LineNumber >= 0 && thread.DiffLineType != DiffChangeType.Delete)
                {
                    var line = snapshot.GetLineFromLineNumber(thread.LineNumber);
                    var p = snapshot.CreateTrackingPoint(line.Start, PointTrackingMode.Positive);
                    result.Add(thread, p);
                }
            }

            return result;
        }

        async Task<string> CalculateCommitSha(
            IPullRequestSession session,
            IPullRequestSessionFile file,
            byte[] content)
        {
            var repo = session.LocalRepository;
            return await sessionService.IsUnmodifiedAndPushed(repo, file.RelativePath, content) ?
                   await sessionService.GetTipSha(repo) : null;
        }

        private void CloseLiveFiles(ITextBuffer textBuffer)
        {
            PullRequestSessionLiveFile file;

            if (textBuffer.Properties.TryGetProperty(
                typeof(IPullRequestSessionFile),
                out file))
            {
                file.Dispose();
            }

            var projection = textBuffer as IProjectionBuffer;

            if (projection != null)
            {
                foreach (var source in projection.SourceBuffers)
                {
                    CloseLiveFiles(source);
                }
            }
        }

        void TextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            var textBuffer = (ITextBuffer)sender;
            var file = textBuffer.Properties.GetProperty<PullRequestSessionLiveFile>(typeof(IPullRequestSessionFile));
            InvalidateLiveThreads(file, e.After);
            file.Rebuild.OnNext(textBuffer.CurrentSnapshot);
        }

        void TextViewClosed(object sender, EventArgs e)
        {
            var textView = (ITextView)sender;
            CloseLiveFiles(textView.TextBuffer);
        }
    }
}
