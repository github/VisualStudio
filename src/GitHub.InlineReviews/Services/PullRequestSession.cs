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
        IPullRequestModel pullRequest;
        string pullRequestNodeId;
        Subject<IPullRequestModel> pullRequestChanged = new Subject<IPullRequestModel>();
        bool hasPendingReview;
        string pendingReviewNodeId { get; set; }

        public PullRequestSession(
            IPullRequestSessionService service,
            IAccount user,
            IPullRequestModel pullRequest,
            ILocalRepositoryModel localRepository,
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
        public async Task<IPullRequestReviewCommentModel> PostReviewComment(
            string body,
            string commitId,
            string path,
            IReadOnlyList<DiffChunk> diff,
            int position)
        {
            IPullRequestReviewCommentModel model;

            if (!HasPendingReview)
            {
                var pullRequestNodeId = await GetPullRequestNodeId();
                model = await service.PostStandaloneReviewComment(
                    LocalRepository,
                    User,
                    pullRequestNodeId,
                    body,
                    commitId,
                    path,
                    position);
            }
            else
            {
                model = await service.PostPendingReviewComment(
                    LocalRepository,
                    User,
                    pendingReviewNodeId,
                    body,
                    commitId,
                    path,
                    position);
            }

            await AddComment(model);
            return model;
        }

        /// <inheritdoc/>
        public async Task DeleteComment(
            int number)
        {
            await service.DeleteComment(
                LocalRepository,
                RepositoryOwner,
                User,
                number);

            await RemoveComment(number);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> EditComment(string commentNodeId, string body)
        {
            var model = await service.EditComment(
                LocalRepository,
                RepositoryOwner,
                User,
                commentNodeId,
                body);

            await ReplaceComment(model);
            return model;
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostReviewComment(
            string body,
            string inReplyToNodeId)
        {
            IPullRequestReviewCommentModel model;

            if (!HasPendingReview)
            {
                var pullRequestNodeId = await GetPullRequestNodeId();
                model = await service.PostStandaloneReviewCommentReply(
                    LocalRepository,
                    User,
                    pullRequestNodeId,
                    body,
                    inReplyToNodeId);
            }
            else
            {
                model = await service.PostPendingReviewCommentReply(
                    LocalRepository,
                    User,
                    pendingReviewNodeId,
                    body,
                    inReplyToNodeId);
            }

            await AddComment(model);
            return model;
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewModel> StartReview()
        {
            if (HasPendingReview)
            {
                throw new InvalidOperationException("A pending review is already underway.");
            }

            var model = await service.CreatePendingReview(
                LocalRepository,
                User,
                await GetPullRequestNodeId());

            await AddReview(model);
            return model;
        }

        /// <inheritdoc/>
        public async Task CancelReview()
        {
            if (!HasPendingReview)
            {
                throw new InvalidOperationException("There is no pending review to cancel.");
            }

            await service.CancelPendingReview(LocalRepository, pendingReviewNodeId);

            PullRequest.Reviews = PullRequest.Reviews
                .Where(x => x.NodeId != pendingReviewNodeId)
                .ToList();
            PullRequest.ReviewComments = PullRequest.ReviewComments
                .Where(x => x.PullRequestReviewId != PendingReviewId)
                .ToList();

            await Update(PullRequest);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewModel> PostReview(string body, Octokit.PullRequestReviewEvent e)
        {
            IPullRequestReviewModel model;

            if (pendingReviewNodeId == null)
            {
                model = await service.PostReview(
                    LocalRepository,
                    RepositoryOwner,
                    User,
                    PullRequest.Number,
                    PullRequest.Head.Sha,
                    body,
                    e);
            }
            else
            {
                model = await service.SubmitPendingReview(
                    LocalRepository,
                    User,
                    pendingReviewNodeId,
                    body,
                    e);
            }

            await AddReview(model);
            return model;
        }

        /// <inheritdoc/>
        public async Task Update(IPullRequestModel pullRequestModel)
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

        async Task AddComment(IPullRequestReviewCommentModel comment)
        {
            PullRequest.ReviewComments = PullRequest.ReviewComments
                .Concat(new[] { comment })
                .ToList();
            await Update(PullRequest);
        }

        async Task ReplaceComment(IPullRequestReviewCommentModel comment)
        {
            PullRequest.ReviewComments = PullRequest.ReviewComments
                .Select(model => model.Id == comment.Id ? comment: model)
                .ToList();

            await Update(PullRequest);
        }

        async Task RemoveComment(int commentId)
        {
            PullRequest.ReviewComments = PullRequest.ReviewComments
                .Where(model => model.Id != commentId)
                .ToList();

            await Update(PullRequest);
        }

        async Task AddReview(IPullRequestReviewModel review)
        {
            PullRequest.Reviews = PullRequest.Reviews
                .Where(x => x.NodeId != review.NodeId)
                .Concat(new[] { review })
                .ToList();

            if (review.State != PullRequestReviewState.Pending)
            {
                foreach (var comment in PullRequest.ReviewComments)
                {
                    if (comment.PullRequestReviewId == review.Id)
                    {
                        comment.IsPending = false;
                    }
                }
            }

            await Update(PullRequest);
        }

        async Task UpdateFile(PullRequestSessionFile file)
        {
            var mergeBaseSha = await GetMergeBase();
            file.BaseSha = PullRequest.Base.Sha;
            file.CommitSha = file.IsTrackingHead ? PullRequest.Head.Sha : file.CommitSha;
            file.Diff = await service.Diff(LocalRepository, mergeBaseSha, file.CommitSha, file.RelativePath);
            file.InlineCommentThreads = service.BuildCommentThreads(PullRequest, file.RelativePath, file.Diff, file.CommitSha);
        }

        void UpdatePendingReview()
        {
            var pendingReview = PullRequest.Reviews
                .FirstOrDefault(x => x.State == PullRequestReviewState.Pending && x.User.Login == User.Login);

            if (pendingReview != null)
            {
                HasPendingReview = true;
                pendingReviewNodeId = pendingReview.NodeId;
                PendingReviewId = pendingReview.Id;
            }
            else
            {
                HasPendingReview = false;
                pendingReviewNodeId = null;
                PendingReviewId = 0;
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
        public IAccount User { get; }

        /// <inheritdoc/>
        public IPullRequestModel PullRequest
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
        public IObservable<IPullRequestModel> PullRequestChanged => pullRequestChanged;

        /// <inheritdoc/>
        public ILocalRepositoryModel LocalRepository { get; }

        /// <inheritdoc/>
        public string RepositoryOwner { get; }

        /// <inheritdoc/>
        public bool HasPendingReview
        {
            get { return hasPendingReview; }
            private set { this.RaiseAndSetIfChanged(ref hasPendingReview, value); }
        }

        /// <inheritdoc/>
        public long PendingReviewId { get; private set; }

        IEnumerable<string> FilePaths
        {
            get { return PullRequest.ChangedFiles.Select(x => x.FileName); }
        }
    }
}
