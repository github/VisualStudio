using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Provides a common interface for services required by <see cref="PullRequestSession"/>.
    /// </summary>
    [Export(typeof(IPullRequestSessionService))]
    class PullRequestSessionService : IPullRequestSessionService
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
        public async Task<IList<DiffChunk>> Diff(ILocalRepositoryModel repository, string baseSha, string headSha, string relativePath, byte[] contents)
        {
            var repo = await GetRepository(repository);
            return await diffService.Diff(repo, baseSha, headSha, relativePath, contents);
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
            mergeBase = await gitClient.GetPullRequestMergeBase(repo, baseUrl, headUrl, baseSha, headSha, baseRef, headRef);
            if (mergeBase != null)
            {
                return mergeBaseCache[key] = mergeBase;
            }

            throw new FileNotFoundException($"Couldn't find merge base between {baseSha} and {headSha}.");
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostReviewComment(
            ILocalRepositoryModel repository,
            IAccount user,
            int number,
            string body,
            string commitId,
            string path,
            int position)
        {
            var address = HostAddress.Create(repository.CloneUrl.Host);
            var apiClient = await apiClientFactory.Create(address);

            var result = await apiClient.CreatePullRequestReviewComment(
                repository.Owner,
                repository.Name,
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
            ILocalRepositoryModel repository,
            IAccount user,
            int number,
            string body,
            int inReplyTo)
        {
            var address = HostAddress.Create(repository.CloneUrl.Host);
            var apiClient = await apiClientFactory.Create(address);

            var result = await apiClient.CreatePullRequestReviewComment(
                repository.Owner,
                repository.Name,
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

        Task<IRepository> GetRepository(ILocalRepositoryModel repository)
        {
            return Task.Factory.StartNew(() => gitService.GetRepository(repository.LocalPath));
        }
    }
}
