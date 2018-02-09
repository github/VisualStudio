using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Factories;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Logging;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using ReactiveUI;
using Serilog;
using PullRequestReviewEvent = Octokit.PullRequestReviewEvent;

// GraphQL DatabaseId field are marked as deprecated, but we need them for interop with REST.
#pragma warning disable CS0618 

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Provides a common interface for services required by <see cref="PullRequestSession"/>.
    /// </summary>
    [Export(typeof(IPullRequestSessionService))]
    public class PullRequestSessionService : IPullRequestSessionService
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestSessionService>();

        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly IApiClientFactory apiClientFactory;
        readonly IGraphQLClientFactory graphqlFactory;
        readonly IUsageTracker usageTracker;

        readonly IDictionary<Tuple<string, string>, string> mergeBaseCache;

        [ImportingConstructor]
        public PullRequestSessionService(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            IApiClientFactory apiClientFactory,
            IGraphQLClientFactory graphqlFactory,
            IUsageTracker usageTracker)
        {
            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            this.apiClientFactory = apiClientFactory;
            this.graphqlFactory = graphqlFactory;
            this.usageTracker = usageTracker;

            mergeBaseCache = new Dictionary<Tuple<string, string>, string>();
        }

        /// <inheritdoc/>
        public virtual async Task<IReadOnlyList<DiffChunk>> Diff(ILocalRepositoryModel repository, string baseSha, string headSha, string relativePath)
        {
            using (var repo = await GetRepository(repository))
            {
                return await diffService.Diff(repo, baseSha, headSha, relativePath);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IReadOnlyList<DiffChunk>> Diff(ILocalRepositoryModel repository, string baseSha, string headSha, string relativePath, byte[] contents)
        {
            using (var repo = await GetRepository(repository))
            {
                return await diffService.Diff(repo, baseSha, headSha, relativePath, contents);
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IInlineCommentThreadModel> BuildCommentThreads(
            IPullRequestModel pullRequest,
            string relativePath,
            IReadOnlyList<DiffChunk> diff)
        {
            relativePath = relativePath.Replace("\\", "/");

            var commentsByPosition = pullRequest.ReviewComments
                .Where(x => x.Path == relativePath && x.OriginalPosition.HasValue)
                .OrderBy(x => x.Id)
                .GroupBy(x => Tuple.Create(x.OriginalCommitId, x.OriginalPosition.Value));
            var threads = new List<IInlineCommentThreadModel>();

            foreach (var comments in commentsByPosition)
            {
                var hunk = comments.First().DiffHunk;
                var chunks = DiffUtilities.ParseFragment(hunk);
                var chunk = chunks.Last();
                var diffLines = chunk.Lines.Reverse().Take(5).ToList();
                var firstLine = diffLines.FirstOrDefault();
                if (firstLine == null)
                {
                    log.Warning("Ignoring in-line comment in {RelativePath} with no diff line context", relativePath);
                    continue;
                }

                var thread = new InlineCommentThreadModel(
                    relativePath,
                    comments.Key.Item1,
                    comments.Key.Item2,
                    diffLines,
                    comments);
                threads.Add(thread);
            }

            UpdateCommentThreads(threads, diff);
            return threads;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Tuple<int, DiffSide>> UpdateCommentThreads(
            IReadOnlyList<IInlineCommentThreadModel> threads,
            IReadOnlyList<DiffChunk> diff)
        {
            var changedLines = new List<Tuple<int, DiffSide>>();

            foreach (var thread in threads)
            {
                var oldLineNumber = thread.LineNumber;
                var newLineNumber = GetUpdatedLineNumber(thread, diff);
                var changed = false;

                if (thread.IsStale)
                {
                    thread.IsStale = false;
                    changed = true;
                }

                if (newLineNumber != thread.LineNumber)
                {
                    thread.LineNumber = newLineNumber;
                    thread.IsStale = false;
                    changed = true;
                }

                if (changed)
                {
                    var side = thread.DiffLineType == DiffChangeType.Delete ? DiffSide.Left : DiffSide.Right;
                    if (oldLineNumber != -1) changedLines.Add(Tuple.Create(oldLineNumber, side));
                    if (newLineNumber != -1 && newLineNumber != oldLineNumber) changedLines.Add(Tuple.Create(newLineNumber, side));
                }
            }

            return changedLines;
        }

        /// <inheritdoc/>
        public byte[] GetContents(ITextBuffer buffer)
        {
            var encoding = GetDocument(buffer)?.Encoding ?? Encoding.Default;
            var content = encoding.GetBytes(buffer.CurrentSnapshot.GetText());

            var preamble = encoding.GetPreamble();
            if (preamble.Length == 0) return content;

            var completeContent = new byte[preamble.Length + content.Length];
            Buffer.BlockCopy(preamble, 0, completeContent, 0, preamble.Length);
            Buffer.BlockCopy(content, 0, completeContent, preamble.Length, content.Length);

            return completeContent;
        }

        /// <inheritdoc/>
        public ITextDocument GetDocument(ITextBuffer buffer)
        {
            ITextDocument result;

            if (buffer.Properties.TryGetProperty(typeof(ITextDocument), out result))
                return result;

            var projection = buffer as IProjectionBuffer;

            if (projection != null)
            {
                foreach (var source in projection.SourceBuffers)
                {
                    if ((result = GetDocument(source)) != null)
                        return result;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public virtual async Task<string> GetTipSha(ILocalRepositoryModel repository)
        {
            using (var repo = await GetRepository(repository))
            {
                return repo.Head.Tip.Sha;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsUnmodifiedAndPushed(ILocalRepositoryModel repository, string relativePath, byte[] contents)
        {
            using (var repo = await GetRepository(repository))
            {
                var modified = await gitClient.IsModified(repo, relativePath, contents);
                var pushed = await gitClient.IsHeadPushed(repo);

                return !modified && pushed;
            }
        }

        public async Task<byte[]> ExtractFileFromGit(
            ILocalRepositoryModel repository,
            int pullRequestNumber,
            string sha,
            string relativePath)
        {
            using (var repo = await GetRepository(repository))
            {
                try
                {
                    return await gitClient.ExtractFileBinary(repo, sha, relativePath);
                }
                catch (FileNotFoundException)
                {
                    var pullHeadRef = $"refs/pull/{pullRequestNumber}/head";
                    await gitClient.Fetch(repo, "origin", sha, pullHeadRef);
                    return await gitClient.ExtractFileBinary(repo, sha, relativePath);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadFileAsync(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    {
                        var buffer = new MemoryStream();
                        await file.CopyToAsync(buffer);
                        return buffer.ToArray();
                    }
                }
                catch { }
            }

            return null;
        }

        public async Task<string> GetGraphQLPullRequestId(
            ILocalRepositoryModel localRepository,
            string repositoryOwner,
            int number)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var query = new Query()
                .Repository(repositoryOwner, localRepository.Name)
                .PullRequest(number)
                .Select(x => x.Id);

            return await graphql.Run(query);
        }

        /// <inheritdoc/>
        public virtual async Task<string> GetPullRequestMergeBase(ILocalRepositoryModel repository, IPullRequestModel pullRequest)
        {
            var baseSha = pullRequest.Base.Sha;
            var headSha = pullRequest.Head.Sha;
            var key = new Tuple<string, string>(baseSha, headSha);

            string mergeBase;
            if (mergeBaseCache.TryGetValue(key, out mergeBase))
            {
                return mergeBase;
            }

            using (var repo = await GetRepository(repository))
            {
                var targetUrl = pullRequest.Base.RepositoryCloneUrl;
                var headUrl = pullRequest.Head.RepositoryCloneUrl;
                var baseRef = pullRequest.Base.Ref;
                var pullNumber = pullRequest.Number;
                try
                {
                    mergeBase = await gitClient.GetPullRequestMergeBase(repo, targetUrl, baseSha, headSha, baseRef, pullNumber);
                }
                catch (NotFoundException ex)
                {
                    throw new NotFoundException("The Pull Request failed to load. Please check your network connection and click refresh to try again. If this issue persists, please let us know at support@github.com", ex);
                }

                return mergeBaseCache[key] = mergeBase;
            }
        }

        /// <inheritdoc/>
        public virtual ISubject<ITextSnapshot, ITextSnapshot> CreateRebuildSignal()
        {
            var input = new Subject<ITextSnapshot>();
            var output = Observable.Create<ITextSnapshot>(x => input
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x));
            return Subject.Create(input, output);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewModel> CreatePendingReview(
            ILocalRepositoryModel localRepository,
            IAccount user,
            string pullRequestId)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var review = new AddPullRequestReviewInput
            {
                PullRequestId = pullRequestId,
            };

            var addReview = new Mutation()
                .AddPullRequestReview(review)
                .Select(x => new PullRequestReviewModel
                {
                    Id = x.PullRequestReview.DatabaseId.Value,
                    Body = x.PullRequestReview.Body,
                    CommitId = x.PullRequestReview.Commit.Oid,
                    NodeId = x.PullRequestReview.Id,
                    State = FromGraphQL(x.PullRequestReview.State),
                    User = user,
                });

            return await graphql.Run(addReview);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewModel> PostReview(
            ILocalRepositoryModel localRepository,
            string remoteRepositoryOwner,
            IAccount user,
            int number,
            string commitId,
            string body,
            PullRequestReviewEvent e)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var apiClient = await apiClientFactory.Create(address);

            var result = await apiClient.PostPullRequestReview(
                remoteRepositoryOwner,
                localRepository.Name,
                number,
                commitId,
                body,
                e);

            return new PullRequestReviewModel
            {
                Id = result.Id,
                Body = result.Body,
                CommitId = result.CommitId,
                State = (GitHub.Models.PullRequestReviewState)result.State.Value,
                User = user,
            };
        }

        public async Task<IPullRequestReviewModel> SubmitPendingReview(
            ILocalRepositoryModel localRepository,
            IAccount user,
            string pendingReviewId,
            string body,
            PullRequestReviewEvent e)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

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
                    State = (GitHub.Models.PullRequestReviewState)x.PullRequestReview.State,
                    User = user,
                });

            return await graphql.Run(mutation);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostPendingReviewComment(
            ILocalRepositoryModel localRepository,
            IAccount user,
            string pendingReviewId,
            string body,
            string commitId,
            string path,
            int position)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

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
                .Select(x => new PullRequestReviewCommentModel
                {
                    Id = x.Comment.DatabaseId.Value,
                    NodeId = x.Comment.Id,
                    Body = x.Comment.Body,
                    CommitId = x.Comment.Commit.Oid,
                    Path = x.Comment.Path,
                    Position = x.Comment.Position,
                    CreatedAt = x.Comment.CreatedAt.Value,
                    DiffHunk = x.Comment.DiffHunk,
                    OriginalPosition = x.Comment.OriginalPosition,
                    OriginalCommitId = x.Comment.OriginalCommit.Oid,
                    PullRequestReviewId = x.Comment.PullRequestReview.DatabaseId.Value,
                    User = user,
                    IsPending = true,
                });

            return await graphql.Run(addComment);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostPendingReviewCommentReply(
            ILocalRepositoryModel localRepository,
            IAccount user,
            string pendingReviewId,
            string body,
            string inReplyTo)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var comment = new AddPullRequestReviewCommentInput
            {
                Body = body,
                InReplyTo = inReplyTo,
                PullRequestReviewId = pendingReviewId,
            };

            var addComment = new Mutation()
                .AddPullRequestReviewComment(comment)
                .Select(x => new PullRequestReviewCommentModel
                {
                    Id = x.Comment.DatabaseId.Value,
                    NodeId = x.Comment.Id,
                    Body = x.Comment.Body,
                    CommitId = x.Comment.Commit.Oid,
                    Path = x.Comment.Path,
                    Position = x.Comment.Position,
                    CreatedAt = x.Comment.CreatedAt.Value,
                    DiffHunk = x.Comment.DiffHunk,
                    OriginalPosition = x.Comment.OriginalPosition,
                    OriginalCommitId = x.Comment.OriginalCommit.Oid,
                    PullRequestReviewId = x.Comment.PullRequestReview.DatabaseId.Value,
                    User = user,
                    IsPending = true,
                });

            return await graphql.Run(addComment);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostStandaloneReviewComment(
            ILocalRepositoryModel localRepository,
            string remoteRepositoryOwner,
            IAccount user,
            int number,
            string body,
            string commitId,
            string path,
            int position)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var apiClient = await apiClientFactory.Create(address);

            var result = await apiClient.CreatePullRequestReviewComment(
                remoteRepositoryOwner,
                localRepository.Name,
                number,
                body,
                commitId,
                path,
                position);

            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);

            return new PullRequestReviewCommentModel
            {
                Body = result.Body,
                CommitId = result.CommitId,
                DiffHunk = result.DiffHunk,
                Id = result.Id,
                OriginalCommitId = result.OriginalCommitId,
                OriginalPosition = result.OriginalPosition,
                Path = result.Path,
                Position = result.Position,
                CreatedAt = result.CreatedAt,
                User = user,
            };
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostReviewCommentRepy(
            ILocalRepositoryModel localRepository,
            string remoteRepositoryOwner,
            IAccount user,
            int number,
            string body,
            int inReplyTo)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var apiClient = await apiClientFactory.Create(address);

            var result = await apiClient.CreatePullRequestReviewComment(
                remoteRepositoryOwner,
                localRepository.Name,
                number,
                body,
                inReplyTo);

            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);

            return new PullRequestReviewCommentModel
            {
                Body = result.Body,
                CommitId = result.CommitId,
                DiffHunk = result.DiffHunk,
                Id = result.Id,
                OriginalCommitId = result.OriginalCommitId,
                OriginalPosition = result.OriginalPosition,
                Path = result.Path,
                Position = result.Position,
                CreatedAt = result.CreatedAt,
                User = user,
            };
        }

        int GetUpdatedLineNumber(IInlineCommentThreadModel thread, IEnumerable<DiffChunk> diff)
        {
            var line = DiffUtilities.Match(diff, thread.DiffMatch);

            if (line != null)
            {
                return (thread.DiffLineType == DiffChangeType.Delete) ?
                    line.OldLineNumber - 1 :
                    line.NewLineNumber - 1;
            }

            return -1;
        }

        Task<IRepository> GetRepository(ILocalRepositoryModel repository)
        {
            return Task.Factory.StartNew(() => gitService.GetRepository(repository.LocalPath));
        }


        static GitHub.Models.PullRequestReviewState FromGraphQL(Octokit.GraphQL.Model.PullRequestReviewState s)
        {
            return (GitHub.Models.PullRequestReviewState)s;
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
    }
}
