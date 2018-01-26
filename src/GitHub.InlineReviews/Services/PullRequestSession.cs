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

// GraphQL DatabaseId field are marked as deprecated, but we need them for interop with REST.
#pragma warning disable CS0618 

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
        readonly Octokit.GraphQL.Connection graphql;
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
            Octokit.GraphQL.Connection graphql,
            IAccount user,
            IPullRequestModel pullRequest,
            ILocalRepositoryModel localRepository,
            string repositoryOwner,
            bool isCheckedOut)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(graphql, nameof(graphql));
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNotNull(pullRequest, nameof(pullRequest));
            Guard.ArgumentNotNull(localRepository, nameof(localRepository));

            this.service = service;
            this.graphql = graphql;
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
            if (!HasPendingReview)
            {
                var model = await service.PostReviewComment(
                    LocalRepository,
                    RepositoryOwner,
                    User,
                    PullRequest.Number,
                    body,
                    commitId,
                    path,
                    position);
                await AddComment(model);
                return model;
            }
            else
            {
                await LoadPullRequestNodeId();

                if (pendingReviewId == null)
                {
                    var review = new AddPullRequestReviewInput
                    {
                        PullRequestId = pullRequestNodeId,
                    };

                    var addReview = new Mutation()
                        .AddPullRequestReview(review)
                        .Select(x => x.PullRequestReview.Id);
                    pendingReviewId = await graphql.Run(addReview);
                }

                var comment = new AddPullRequestReviewCommentInput
                {
                    Body = body,
                    CommitOID = commitId,
                    Path = path,
                    Position = position,
                    PullRequestReviewId = pendingReviewId,
                };

                var addComment = new Mutation()
                    .AddPullRequestReviewComment(comment)
                    .Select(x => x.Comment.DatabaseId);
                var commentId = await graphql.Run(addComment);

                var model = new PullRequestReviewCommentModel
                {
                    Id = commentId.Value,
                    Body = body,
                    CommitId = commitId,
                    Path = path,
                    Position = position,
                    CreatedAt = DateTimeOffset.Now,
                    DiffHunk = BuildDiffHunk(diff, position),
                    OriginalPosition = position,
                    OriginalCommitId = commitId,
                    User = User,
                };

                await AddComment(model);
                return model;
            }
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostReviewComment(string body, int inReplyTo)
        {
            var model = await service.PostReviewComment(
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
        public void StartReview()
        {
            if (!HasPendingReview)
            {
                var newReview = new PullRequestReviewModel
                {
                    Id = -1,
                    State = Octokit.PullRequestReviewState.Pending,
                    User = User,
                };

                PullRequest.Reviews = PullRequest.Reviews.Concat(new[] { newReview }).ToList();
                HasPendingReview = true;
                pendingReviewId = null;
            }
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewModel> PostReview(string body, Octokit.PullRequestReviewEvent e)
        {
            if (pendingReviewId == null)
            {
                await LoadPullRequestNodeId();

                var review = new AddPullRequestReviewInput
                {
                    Body = body,
                    Event = ToGraphQl(e),
                    PullRequestId = pullRequestNodeId,
                };

                var mutation = new Mutation()
                    .AddPullRequestReview(review)
                    .Select(x => new PullRequestReviewModel
                    {
                        Body = body,
                        CommitId = x.PullRequestReview.Commit.Oid,
                        Id = x.PullRequestReview.DatabaseId.Value,
                        NodeId = x.PullRequestReview.Id,
                        State = ToRest(x.PullRequestReview.State),
                        User = User,
                    });

                var model = await graphql.Run(mutation);
                PullRequest.Reviews = PullRequest.Reviews.Concat(new[] { model }).ToList();
                HasPendingReview = false;
                return model;
            }
            else
            {
                var submit = new SubmitPullRequestReviewInput
                {
                    Body = body,
                    Event = ToGraphQl(e),
                    PullRequestReviewId = pendingReviewId,
                };

                var mutation = new Mutation()
                    .SubmitPullRequestReview(submit)
                    .Select(x => new PullRequestReviewModel
                    {
                        Body = body,
                        CommitId = x.PullRequestReview.Commit.Oid,
                        Id = x.PullRequestReview.DatabaseId.Value,
                        NodeId = x.PullRequestReview.Id,
                        State = ToRest(x.PullRequestReview.State),
                        User = User,
                    });

                var model = await graphql.Run(mutation);
                PullRequest.Reviews = PullRequest.Reviews.Concat(new[] { model }).ToList();
                HasPendingReview = false;
                pendingReviewId = null;
                return model;
            }
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

        async Task LoadPullRequestNodeId()
        {
            if (pullRequestNodeId == null)
            {
                var query = new Query()
                    .Repository(RepositoryOwner, LocalRepository.Name)
                    .PullRequest(PullRequest.Number)
                    .Select(x => x.Id);
                pullRequestNodeId = await graphql.Run(query);
            }
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

        static Octokit.PullRequestReviewState ToRest(Octokit.GraphQL.Model.PullRequestReviewState state)
        {
            switch (state)
            {
                case Octokit.GraphQL.Model.PullRequestReviewState.Approved:
                    return Octokit.PullRequestReviewState.Approved;
                case Octokit.GraphQL.Model.PullRequestReviewState.ChangesRequested:
                    return Octokit.PullRequestReviewState.ChangesRequested;
                case Octokit.GraphQL.Model.PullRequestReviewState.Commented:
                    return Octokit.PullRequestReviewState.Commented;
                case Octokit.GraphQL.Model.PullRequestReviewState.Dismissed:
                    return Octokit.PullRequestReviewState.Dismissed;
                case Octokit.GraphQL.Model.PullRequestReviewState.Pending:
                    return Octokit.PullRequestReviewState.Pending;
                default:
                    throw new NotSupportedException();
            }
        }

        static Octokit.GraphQL.Model.PullRequestReviewEvent ToGraphQl(Octokit.PullRequestReviewEvent e)
        {
            switch (e)
            {
                case Octokit.PullRequestReviewEvent.Approve:
                    return Octokit.GraphQL.Model.PullRequestReviewEvent.Approve;
                case Octokit.PullRequestReviewEvent.Comment:
                    return Octokit.GraphQL.Model.PullRequestReviewEvent.Comment;
                case Octokit.PullRequestReviewEvent.RequestChanges:
                    return Octokit.GraphQL.Model.PullRequestReviewEvent.RequestChanges;
                default:
                    throw new NotSupportedException();
            }
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
