using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using System.Threading;
using System.Reactive.Subjects;
using static System.FormattableString;
using GitHub.Primitives;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// A pull request session used to display inline reviews.
    /// </summary>
    /// <remarks>
    /// A pull request session represents the real-time state of a pull request in the IDE.
    /// It takes the pull request model and updates according to the current state of the
    /// repository on disk and in the editor.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "PullRequestSession is shared and shouldn't be disposed")]
    public class PullRequestSession : ReactiveObject, IPullRequestSession
    {
        readonly IPullRequestSessionService service;
        readonly Dictionary<string, PullRequestSessionFile> fileIndex = new Dictionary<string, PullRequestSessionFile>();
        readonly SemaphoreSlim getFilesLock = new SemaphoreSlim(1);
        bool isCheckedOut;
        string mergeBase;
        IReadOnlyList<IPullRequestSessionFile> files;
        PullRequestDetailModel pullRequest;
        string pullRequestNodeId;
        Subject<PullRequestDetailModel> pullRequestChanged = new Subject<PullRequestDetailModel>();
        bool hasPendingReview;

        public PullRequestSession(
            IPullRequestSessionService service,
            ActorModel user,
            PullRequestDetailModel pullRequest,
            LocalRepositoryModel localRepository,
            string repositoryOwner,
            bool isCheckedOut)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNotNull(pullRequest, nameof(pullRequest));
            Guard.ArgumentNotNull(localRepository, nameof(localRepository));

            this.service = service;
            this.isCheckedOut = isCheckedOut;
            this.pullRequest = pullRequest;
            User = user;
            LocalRepository = localRepository;
            RepositoryOwner = repositoryOwner;
            UpdatePendingReview();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<IPullRequestSessionFile>> GetAllFiles()
        {
            if (files == null)
            {
                files = await CreateAllFiles();
            }

            return files;
        }

        /// <inheritdoc/>
        public async Task<IPullRequestSessionFile> GetFile(
            string relativePath,
            string commitSha = "HEAD")
        {
            await getFilesLock.WaitAsync();

            try
            {
                PullRequestSessionFile file;
                var normalizedPath = relativePath.Replace("\\", "/");
                var key = normalizedPath + '@' + commitSha;

                if (!fileIndex.TryGetValue(key, out file))
                {
                    file = new PullRequestSessionFile(normalizedPath, commitSha);
                    await UpdateFile(file);
                    fileIndex.Add(key, file);
                }

                return file;
            }
            finally
            {
                getFilesLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetMergeBase()
        {
            if (mergeBase == null)
            {
                mergeBase = await service.GetPullRequestMergeBase(LocalRepository, PullRequest);
            }

            return mergeBase;
        }

        /// <inheritdoc/>
        public string GetRelativePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                var basePath = LocalRepository.LocalPath;

                if (path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) && path.Length > basePath.Length + 1)
                {
                    return path.Substring(basePath.Length + 1);
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task PostReviewComment(
            string body,
            string commitId,
            string path,
            IReadOnlyList<DiffChunk> diff,
            int position)
        {
            if (!HasPendingReview)
            {
                var model = await service.PostStandaloneReviewComment(
                    LocalRepository,
                    PullRequest.Id,
                    body,
                    commitId,
                    path,
                    position);
                await Update(model);
            }
            else
            {
                var model = await service.PostPendingReviewComment(
                    LocalRepository,
                    PendingReviewId,
                    body,
                    commitId,
                    path,
                    position);
                await Update(model);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteComment(int pullRequestId, int commentDatabaseId)
        {
            var model = await service.DeleteComment(
                LocalRepository,
                RepositoryOwner,
                pullRequestId,
                commentDatabaseId);

            await Update(model);
        }

        /// <inheritdoc/>
        public async Task EditComment(string commentNodeId, string body)
        {
            var model = await service.EditComment(
                LocalRepository,
                RepositoryOwner,
                commentNodeId,
                body);

            await Update(model);
        }

        /// <inheritdoc/>
        public async Task PostReviewComment(
            string body,
            string inReplyTo)
        {
            if (!HasPendingReview)
            {
                var model = await service.PostStandaloneReviewCommentReply(
                    LocalRepository,
                    PullRequest.Id,
                    body,
                    inReplyTo);
                await Update(model);
            }
            else
            {
                var model = await service.PostPendingReviewCommentReply(
                    LocalRepository,
                    PendingReviewId,
                    body,
                    inReplyTo);
                await Update(model);
            }
        }

        /// <inheritdoc/>
        public async Task StartReview()
        {
            if (HasPendingReview)
            {
                throw new InvalidOperationException("A pending review is already underway.");
            }

            var model = await service.CreatePendingReview(
                LocalRepository,
                await GetPullRequestNodeId());

            await Update(model);
        }

        /// <inheritdoc/>
        public async Task CancelReview()
        {
            if (!HasPendingReview)
            {
                throw new InvalidOperationException("There is no pending review to cancel.");
            }

            var pullRequest = await service.CancelPendingReview(LocalRepository, PendingReviewId);
            await Update(pullRequest);
        }

        /// <inheritdoc/>
        public async Task PostReview(string body, Octokit.PullRequestReviewEvent e)
        {
            PullRequestDetailModel model;

            if (PendingReviewId == null)
            {
                model = await service.PostReview(
                    LocalRepository,
                    PullRequest.Id,
                    PullRequest.HeadRefSha,
                    body,
                    e);
            }
            else
            {
                model = await service.SubmitPendingReview(
                    LocalRepository,
                    PendingReviewId,
                    body,
                    e);
            }

            await Update(model);
        }

        /// <inheritdoc/>
        public async Task Refresh()
        {
            var address = HostAddress.Create(LocalRepository.CloneUrl);
            var model = await service.ReadPullRequestDetail(
                address,
                RepositoryOwner,
                LocalRepository.Name,
                PullRequest.Number);
            await Update(model);
        }

        /// <inheritdoc/>
        async Task Update(PullRequestDetailModel pullRequestModel)
        {
            PullRequest = pullRequestModel;
            mergeBase = null;

            foreach (var file in this.fileIndex.Values.ToList())
            {
                await UpdateFile(file);
            }

            UpdatePendingReview();
            pullRequestChanged.OnNext(pullRequestModel);
        }

        async Task AddComment(PullRequestReviewCommentModel comment)
        {
            var review = PullRequest.Reviews.FirstOrDefault(x => x.Id == PendingReviewId);

            if (review == null)
            {
                throw new KeyNotFoundException("Could not find pending review.");
            }

            review.Comments = review.Comments
                .Concat(new[] { comment })
                .ToList();
            await Update(PullRequest);
        }

        async Task UpdateFile(PullRequestSessionFile file)
        {
            await Task.Delay(0);
            var mergeBaseSha = await GetMergeBase();
            file.BaseSha = PullRequest.BaseRefSha;
            file.CommitSha = file.IsTrackingHead ? PullRequest.HeadRefSha : file.CommitSha;
            file.Diff = await service.Diff(LocalRepository, mergeBaseSha, file.CommitSha, file.RelativePath);
            file.InlineCommentThreads = service.BuildCommentThreads(PullRequest, file.RelativePath, file.Diff, file.CommitSha);
            file.InlineAnnotations = service.BuildAnnotations(PullRequest, file.RelativePath);
        }

        void UpdatePendingReview()
        {
            var pendingReview = PullRequest.Reviews
                .FirstOrDefault(x => x.State == PullRequestReviewState.Pending && x.Author.Login == User.Login);

            if (pendingReview != null)
            {
                HasPendingReview = true;
                PendingReviewId = pendingReview.Id;
            }
            else
            {
                HasPendingReview = false;
                PendingReviewId = null;
            }
        }

        async Task<IReadOnlyList<IPullRequestSessionFile>> CreateAllFiles()
        {
            var result = new List<IPullRequestSessionFile>();

            foreach (var path in FilePaths)
            {
                var file = await GetFile(path);
                result.Add(file);
            }

            return result;
        }

        string GetFullPath(string relativePath)
        {
            return Path.Combine(LocalRepository.LocalPath, relativePath);
        }

        async Task<string> GetPullRequestNodeId()
        {
            if (pullRequestNodeId == null)
            {
                pullRequestNodeId = await service.GetGraphQLPullRequestId(
                    LocalRepository,
                    RepositoryOwner,
                    PullRequest.Number);
            }

            return pullRequestNodeId;
        }

        static string BuildDiffHunk(IReadOnlyList<DiffChunk> diff, int position)
        {
            var lines = diff.SelectMany(x => x.Lines).Reverse();
            var context = lines.SkipWhile(x => x.DiffLineNumber != position).Take(5).Reverse().ToList();
            var oldLineNumber = context.Select(x => x.OldLineNumber).Where(x => x != -1).FirstOrDefault();
            var newLineNumber = context.Select(x => x.NewLineNumber).Where(x => x != -1).FirstOrDefault();
            var header = Invariant($"@@ -{oldLineNumber},5 +{newLineNumber},5 @@");
            return header + '\n' + string.Join("\n", context);
        }

        /// <inheritdoc/>
        public bool IsCheckedOut
        {
            get { return isCheckedOut; }
            internal set { this.RaiseAndSetIfChanged(ref isCheckedOut, value); }
        }

        /// <inheritdoc/>
        public ActorModel User { get; }

        /// <inheritdoc/>
        public PullRequestDetailModel PullRequest
        {
            get { return pullRequest; }
            private set
            {
                // PullRequestModel overrides Equals such that two PRs with the same number are
                // considered equal. This was causing the PullRequest not to be updated on refresh:
                // we need to use ReferenceEquals.
                if (!ReferenceEquals(pullRequest, value))
                {
                    this.RaisePropertyChanging(nameof(PullRequest));
                    pullRequest = value;
                    this.RaisePropertyChanged(nameof(PullRequest));
                }
            }
        }

        /// <inheritdoc/>
        public IObservable<PullRequestDetailModel> PullRequestChanged => pullRequestChanged;

        /// <inheritdoc/>
        public LocalRepositoryModel LocalRepository { get; }

        /// <inheritdoc/>
        public string RepositoryOwner { get; }

        /// <inheritdoc/>
        public bool HasPendingReview
        {
            get { return hasPendingReview; }
            private set { this.RaiseAndSetIfChanged(ref hasPendingReview, value); }
        }

        /// <inheritdoc/>
        public string PendingReviewId { get; private set; }

        IEnumerable<string> FilePaths
        {
            get { return PullRequest.ChangedFiles.Select(x => x.FileName); }
        }
    }
}
