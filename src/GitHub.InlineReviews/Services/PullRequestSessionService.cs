using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;

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

        readonly IDictionary<Tuple<string, string>, string> mergeBaseCache;

        [ImportingConstructor]
        public PullRequestSessionService(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService)
        {
            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;

            mergeBaseCache = new Dictionary<Tuple<string, string>, string>();
        }

        /// <inheritdoc/>
        public Task<IList<DiffChunk>> Diff(ILocalRepositoryModel repository, string baseSha, string relativePath, byte[] contents)
        {
            var repo = gitService.GetRepository(repository.LocalPath);
            return diffService.Diff(repo, baseSha, relativePath, contents);
        }

        /// <inheritdoc/>
        public string GetTipSha(ILocalRepositoryModel repository)
        {
            var repo = gitService.GetRepository(repository.LocalPath);
            return repo.Head.Tip.Sha;
        }

        /// <inheritdoc/>
        public async Task<bool> IsUnmodifiedAndPushed(ILocalRepositoryModel repository, string relativePath, byte[] contents)
        {
            var repo = gitService.GetRepository(repository.LocalPath);

            return !await gitClient.IsModified(repo, relativePath, contents) &&
                   await gitClient.IsHeadPushed(repo);
        }

        public async Task<byte[]> ExtractFileFromGit(
            ILocalRepositoryModel repository,
            int pullRequestNumber,
            string sha,
            string relativePath)
        {
            var repo = gitService.GetRepository(repository.LocalPath);

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
            if(mergeBaseCache.TryGetValue(key, out mergeBase))
            {
                return mergeBase;
            }

            var repo = gitService.GetRepository(repository.LocalPath);
            var remote = await gitClient.GetHttpRemote(repo, "origin");

            var baseRef = pullRequest.Base.Ref;
            var pullNumber = pullRequest.Number;
            mergeBase = await gitClient.GetPullRequestMergeBase(repo, remote.Name, baseSha, headSha, baseRef, pullNumber);
            if (mergeBase == null)
            {
                throw new FileNotFoundException($"Couldn't find merge base between {baseSha} and {headSha}.");
            }

            mergeBaseCache[key] = mergeBase;
            return mergeBase;
        }
    }
}
