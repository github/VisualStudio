using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using ReactiveUI;
using System.Threading;

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
    public class PullRequestSession : IPullRequestSession
    {
        static readonly List<IPullRequestReviewCommentModel> Empty = new List<IPullRequestReviewCommentModel>();
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly Dictionary<string, PullRequestSessionFile> fileIndex = new Dictionary<string, PullRequestSessionFile>();
        readonly SemaphoreSlim getFilesLock = new SemaphoreSlim(1);
        ReactiveList<IPullRequestSessionFile> files;
        bool? isCheckedOut;

        public PullRequestSession(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            IAccount user,
            IPullRequestModel pullRequest,
            ILocalRepositoryModel repository,
            bool isCheckedOut)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNotNull(pullRequest, nameof(pullRequest));
            Guard.ArgumentNotNull(repository, nameof(repository));

            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            IsCheckedOut = isCheckedOut;
            User = user;
            PullRequest = pullRequest;
            Repository = repository;
        }

        /// <inheritdoc/>
        public bool IsCheckedOut { get; }

        /// <inheritdoc/>
        public IAccount User { get; }

        /// <inheritdoc/>
        public IPullRequestModel PullRequest { get; }

        /// <inheritdoc/>
        public ILocalRepositoryModel Repository { get; }

        IEnumerable<string> FilePaths
        {
            get { return PullRequest.ChangedFiles.Select(x => x.FileName); }
        }

        /// <inheritdoc/>
        public async Task<IReactiveList<IPullRequestSessionFile>> GetAllFiles()
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
            var contents = await ReadAllTextAsync(GetFullPath(relativePath));
            return await GetFile(relativePath, contents);
        }

        /// <inheritdoc/>
        public Task<IPullRequestSessionFile> GetFile(string relativePath, ITextSnapshot snapshot)
        {
            var contents = snapshot.GetText();
            return GetFile(relativePath, contents);
        }

        /// <inheritdoc/>
        public async Task RecaluateLineNumbers(string relativePath, string contents)
        {
            relativePath = relativePath.Replace("\\", "/");

            var updated = await Task.Run(() =>
            {
                PullRequestSessionFile file;

                if (fileIndex.TryGetValue(relativePath, out file))
                {
                    var result = new Dictionary<IInlineCommentThreadModel, int>();

                    file.Diff = diffService.Diff(file.BaseCommit, contents).ToList();

                    foreach (var thread in file.InlineCommentThreads)
                    {
                        result[thread] = GetUpdatedLineNumber(thread, file.Diff);
                    }

                    return result;
                }

                return null;
            });

            if (updated != null)
            {
                foreach (var i in updated)
                {
                    i.Key.LineNumber = i.Value;
                    i.Key.IsStale = false;
                }
            }
        }

        async Task<IPullRequestSessionFile> GetFile(string relativePath, string contents)
        {
            await getFilesLock.WaitAsync();

            try
            {
                PullRequestSessionFile file;

                relativePath = relativePath.Replace("\\", "/");

                if (!fileIndex.TryGetValue(relativePath, out file))
                {
                    // TODO: Check for binary files.
                    if (FilePaths.Any(x => x == relativePath))
                    {
                        file = await CreateFile(relativePath, contents);
                        fileIndex.Add(relativePath, file);
                    }
                    else
                    {
                        fileIndex.Add(relativePath, null);
                    }
                }

                return file;
            }
            finally
            {
                getFilesLock.Release();
            }
        }

        async Task<PullRequestSessionFile> CreateFile(string relativePath, string contents)
        {
            var file = new PullRequestSessionFile();
            var repository = gitService.GetRepository(Repository.LocalPath);

            file.RelativePath = relativePath;
            file.BaseSha = PullRequest.Base.Sha;
            file.BaseCommit = await gitClient.ExtractFile(repository, file.BaseSha, relativePath);
            file.CommitSha = await gitClient.IsModified(repository, relativePath, contents) ? null : repository.Head.Tip.Sha;
            file.Diff = diffService.Diff(file.BaseCommit, contents).ToList();

            var commentsByPosition = PullRequest.ReviewComments
                .Where(x => x.Path == relativePath && x.OriginalPosition.HasValue)
                .OrderBy(x => x.Id)
                .GroupBy(x => Tuple.Create(x.OriginalCommitId, x.OriginalPosition.Value));

            foreach (var position in commentsByPosition)
            {
                var chunk = diffService.ParseFragment(position.First().DiffHunk);
                var diffLines = chunk.Last().Lines.Reverse().Take(5).ToList();
                var thread = new InlineCommentThreadModel(
                    relativePath,
                    position.Key.Item1,
                    position.Key.Item2,
                    diffLines);
                thread.Comments.AddRange(position);
                thread.LineNumber = GetUpdatedLineNumber(thread, file.Diff);
                file.InlineCommentThreads.Add(thread);
            }

            return file;
        }

        async Task<ReactiveList<IPullRequestSessionFile>> CreateAllFiles()
        {
            var result = new ReactiveList<IPullRequestSessionFile>();

            foreach (var path in FilePaths)
            {
                var contents = await ReadAllTextAsync(GetFullPath(path));
                result.Add(await CreateFile(path, contents));
            }

            return result;
        }

        string GetFullPath(string relativePath)
        {
            return Path.Combine(Repository.LocalPath, relativePath);
        }

        int GetUpdatedLineNumber(IInlineCommentThreadModel thread, IEnumerable<DiffChunk> diff)
        {
            var line = Match(diff, thread.DiffMatch);

            if (line != null)
            {
                return (thread.DiffLineType == DiffChangeType.Delete) ?
                    line.OldLineNumber - 1 :
                    line.NewLineNumber - 1;
            }

            return -1;
        }

        static DiffLine Match(IEnumerable<DiffChunk> diff, IList<DiffLine> target)
        {
            int j = 0;

            foreach (var source in diff)
            {
                for (var i = source.Lines.Count - 1; i >= 0; --i)
                {
                    if (source.Lines[i].Content == target[j].Content)
                    {
                        if (++j == target.Count) return source.Lines[i + j - 1];
                    }
                    else
                    {
                        j = 0;
                    }
                }
            }

            return null;
        }

        static async Task<string> ReadAllTextAsync(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var reader = File.OpenText(path))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                catch { }
            }

            return null;
        }
    }
}
