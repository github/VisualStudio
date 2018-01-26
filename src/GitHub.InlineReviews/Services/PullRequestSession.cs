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
using Octokit.GraphQL;
using Octokit.GraphQL.Model;

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
        string pendingReviewId { get; set; }

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
        public async Task<IPullRequestSessionFile> GetFile(string relativePath)
        {
            await getFilesLock.WaitAsync();

            try
            {
                PullRequestSessionFile file;

                relativePath = relativePath.Replace("\\", "/");

                if (!fileIndex.TryGetValue(relativePath, out file))
                {
                    file = new PullRequestSessionFile(relativePath);
                    await UpdateFile(file);
                    fileIndex.Add(relativePath, file);
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
                model = await service.PostStandaloneReviewComment(
                    LocalRepository,
                    RepositoryOwner,
                    User,
                    PullRequest.Number,
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
                    pendingReviewId,
                    body,
                    commitId,
                    path,
                    position);
            }

            await AddComment(model);
            return model;
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostReviewComment(string body, int inReplyTo)
        {
            var model = await service.PostReviewCommentRepy(
                LocalRepository,
                RepositoryOwner,
                User,
                PullRequest.Number,
                body,
                inReplyTo);
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
            HasPendingReview = true;
            pendingReviewId = model.NodeId;
            return model;
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewModel> PostReview(string body, Octokit.PullRequestReviewEvent e)
        {
            IPullRequestReviewModel model;

            if (pendingReviewId == null)
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
                    pendingReviewId,
                    body,
                    e);
            }

            PullRequest.Reviews = PullRequest.Reviews.Concat(new[] { model }).ToList();
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

            var pendingReview = pullRequestModel.Reviews
                .FirstOrDefault(x => x.State == Octokit.PullRequestReviewState.Pending && x.User.Equals(User));

            if (pendingReview != null)
            {
                HasPendingReview = true;

                if (pendingReview.NodeId != null)
                {
                    pendingReviewId = pendingReview.NodeId;
                }
                else
                {
                    // TODO: REST->GraphQL mapping not yet implemented on this instance. Display a
                    // warning telling the user to finish their review online or something.
                }
            }

            pullRequestChanged.OnNext(pullRequestModel);
        }

        async Task AddComment(IPullRequestReviewCommentModel comment)
        {
            PullRequest.ReviewComments = PullRequest.ReviewComments
                .Concat(new[] { comment })
                .ToList();
            await Update(PullRequest);
        }

        async Task UpdateFile(PullRequestSessionFile file)
        {
            var mergeBaseSha = await GetMergeBase();
            file.BaseSha = PullRequest.Base.Sha;
            file.CommitSha = PullRequest.Head.Sha;
            file.Diff = await service.Diff(LocalRepository, mergeBaseSha, file.CommitSha, file.RelativePath);
            file.InlineCommentThreads = service.BuildCommentThreads(PullRequest, file.RelativePath, file.Diff);
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
