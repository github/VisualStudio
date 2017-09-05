using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using NLog;
using ReactiveUI;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Provides a common interface for services required by <see cref="PullRequestSession"/>.
    /// </summary>
    [Export(typeof(IPullRequestSessionService))]
    public class PullRequestSessionService : IPullRequestSessionService
    {
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly IApiClientFactory apiClientFactory;
        readonly IUsageTracker usageTracker;

        readonly IDictionary<Tuple<string, string>, string> mergeBaseCache;

        [ImportingConstructor]
        public PullRequestSessionService(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            IApiClientFactory apiClientFactory,
            IUsageTracker usageTracker)
        {
            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            this.apiClientFactory = apiClientFactory;
            this.usageTracker = usageTracker;

            mergeBaseCache = new Dictionary<Tuple<string, string>, string>();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<DiffChunk>> Diff(ILocalRepositoryModel repository, string baseSha, string headSha, string relativePath, byte[] contents)
        {
            var repo = await GetRepository(repository);
            return await diffService.Diff(repo, baseSha, headSha, relativePath, contents);
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
        public IReadOnlyList<int> UpdateCommentThreads(
            IReadOnlyList<IInlineCommentThreadModel> threads,
            IReadOnlyList<DiffChunk> diff)
        {
            var changedLines = new List<int>();

            foreach (var thread in threads)
            {
                var hunk = thread.Comments.First().DiffHunk;
                var chunks = DiffUtilities.ParseFragment(hunk);
                var chunk = chunks.Last();
                var diffLines = chunk.Lines.Reverse().Take(5).ToList();
                var newLineNumber = GetUpdatedLineNumber(thread, diff);

                if (newLineNumber != thread.LineNumber)
                {
                    if (thread.LineNumber != -1) changedLines.Add(thread.LineNumber);
                    if (newLineNumber != -1) changedLines.Add(newLineNumber);
                    thread.LineNumber = newLineNumber;
                }
            }

            return changedLines;
        }

        /// <inheritdoc/>
        public byte[] GetContents(ITextBuffer buffer)
        {
            var encoding = GetDocument(buffer).Encoding ?? Encoding.Default;
            return encoding.GetBytes(buffer.CurrentSnapshot.GetText());
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
        public async Task<string> GetTipSha(ILocalRepositoryModel repository)
        {
            var repo = await GetRepository(repository);
            return repo.Head.Tip.Sha;
        }

        /// <inheritdoc/>
        public async Task<bool> IsUnmodifiedAndPushed(ILocalRepositoryModel repository, string relativePath, byte[] contents)
        {
            var repo = await GetRepository(repository);
            var modified = await gitClient.IsModified(repo, relativePath, contents);
            var pushed = await gitClient.IsHeadPushed(repo);

            return !modified && pushed;
        }

        public async Task<byte[]> ExtractFileFromGit(
            ILocalRepositoryModel repository,
            int pullRequestNumber,
            string sha,
            string relativePath)
        {
            var repo = await GetRepository(repository);

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

        /// <inheritdoc/>
        public async Task<string> GetPullRequestMergeBase(ILocalRepositoryModel repository, IPullRequestModel pullRequest)
        {
            var baseSha = pullRequest.Base.Sha;
            var headSha = pullRequest.Head.Sha;
            var key = new Tuple<string, string>(baseSha, headSha);

            string mergeBase;
            if (mergeBaseCache.TryGetValue(key, out mergeBase))
            {
                return mergeBase;
            }

            var repo = await GetRepository(repository);
            var baseUrl = pullRequest.Base.RepositoryCloneUrl;
            var headUrl = pullRequest.Head.RepositoryCloneUrl;
            var baseRef = pullRequest.Base.Ref;
            var headRef = pullRequest.Head.Ref;
            try
            {
                mergeBase = await gitClient.GetPullRequestMergeBase(repo, baseUrl, headUrl, baseSha, headSha, baseRef, headRef);
            }
            catch (NotFoundException ex)
            {
                throw new NotFoundException("The Pull Request failed to load. Please check your network connection and click refresh to try again. If this issue persists, please let us know at support@github.com", ex);
            }

            return mergeBaseCache[key] = mergeBase;
        }

        public ISubject<ITextSnapshot, ITextSnapshot> CreateRebuildSignal()
        {
            var input = new Subject<ITextSnapshot>();
            var output = Observable.Create<ITextSnapshot>(x => input
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x));
            return Subject.Create(input, output);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostReviewComment(
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

            await usageTracker.IncrementPRReviewDiffViewInlineCommentPost();

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
        public async Task<IPullRequestReviewCommentModel> PostReviewComment(
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

            await usageTracker.IncrementPRReviewDiffViewInlineCommentPost();

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
    }
}
