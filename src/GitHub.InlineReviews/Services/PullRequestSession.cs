using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.InlineReviews.Models;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.InlineReviews.Services
{
    public class PullRequestSession : IPullRequestSession, IDisposable
    {
        static readonly List<IPullRequestReviewCommentModel> Empty = new List<IPullRequestReviewCommentModel>();
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly Subject<Unit> changed = new Subject<Unit>();
        readonly Dictionary<string, List<IPullRequestReviewCommentModel>> pullRequestComments;
        readonly Dictionary<string, PullRequestSessionFile> fileIndex = new Dictionary<string, PullRequestSessionFile>();
        ReactiveList<IPullRequestSessionFile> files;

        public PullRequestSession(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            IAccount user,
            IPullRequestModel pullRequest,
            ILocalRepositoryModel repository)
        {
            Guard.ArgumentNotNull(gitService, nameof(gitService));
            Guard.ArgumentNotNull(gitClient, nameof(gitClient));
            Guard.ArgumentNotNull(user, nameof(user));
            Guard.ArgumentNotNull(pullRequest, nameof(pullRequest));
            Guard.ArgumentNotNull(repository, nameof(repository));

            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            User = user;
            PullRequest = pullRequest;
            Repository = repository;
            pullRequestComments = pullRequest.ReviewComments
                .OrderBy(x => x.Id)
                .GroupBy(x => x.Path)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public IAccount User { get; }
        public IPullRequestModel PullRequest { get; }
        public ILocalRepositoryModel Repository { get; }
        public IObservable<Unit> Changed => changed;

        private IEnumerable<string> FilePaths
        {
            get { return PullRequest.ChangedFiles.Select(x => x.FileName); }
        }

        public void AddComment(IPullRequestReviewCommentModel comment)
        {
            List<IPullRequestReviewCommentModel> fileComments;

            if (!pullRequestComments.TryGetValue(comment.Path, out fileComments))
            {
                fileComments = new List<IPullRequestReviewCommentModel>();
                pullRequestComments.Add(comment.Path, fileComments);
            }

            fileComments.Add(comment);
            changed.OnNext(Unit.Default);
        }

        public IReadOnlyList<IPullRequestReviewCommentModel> GetCommentsForFile(string path)
        {
            List<IPullRequestReviewCommentModel> result;
            path = path.Replace('\\', '/');
            return pullRequestComments.TryGetValue(path, out result) ? result : Empty;
        }

        public async Task<IPullRequestSessionFile> GetFile(string path)
        {
            PullRequestSessionFile file;

            if (!fileIndex.TryGetValue(path, out file))
            {
                // TODO: Check for binary files.
                if (FilePaths.Any(x => x == path))
                {
                    file = await CreateFile(path);
                    fileIndex.Add(path, file);
                }
                else
                {
                    fileIndex.Add(path, null);
                }
            }

            return file;
        }

        public async Task<IReactiveList<IPullRequestSessionFile>> GetAllFiles()
        {
            if (files == null)
            {
                files = await CreateFiles();
            }

            return files;
        }

        public void Dispose()
        {
            changed.Dispose();
            GC.SuppressFinalize(this);
        }

        async Task<PullRequestSessionFile> CreateFile(string path)
        {
            var result = new PullRequestSessionFile();
            var repository = gitService.GetRepository(Repository.LocalPath);
            var contents = await ReadAllTextAsync(path);

            result.RelativePath = path;
            result.BaseSha = PullRequest.Base.Sha;
            result.BaseCommit = await gitClient.ExtractFile(repository, result.BaseSha, path);
            result.Diff = diffService.Diff(result.BaseCommit, contents).ToList();

            var commentsByPosition = PullRequest.ReviewComments
                .Where(x => x.Path == path)
                .OrderBy(x => x.Id)
                .GroupBy(x => Tuple.Create(x.OriginalCommitId, x.OriginalPosition));

            foreach (var position in commentsByPosition)
            {
                var chunk = diffService.ParseFragment(position.First().DiffHunk);
                var diffLines = chunk.Last().Lines.Reverse().Take(5).ToList();
                var thread = new InlineCommentThreadModel(diffLines);
                thread.Comments.AddRange(position);
                result.InlineCommentThreads.Add(thread);
            }

            return result;
        }

        async Task<ReactiveList<IPullRequestSessionFile>> CreateFiles()
        {
            var result = new ReactiveList<IPullRequestSessionFile>();

            foreach (var path in FilePaths)
            {
                result.Add(await CreateFile(path));
            }

            return result;
        }

        void UpdateLineNumber(InlineCommentThreadModel thread, IEnumerable<DiffChunk> diff)
        {
            var line = Match(diff, thread.DiffMatch);

            if (line != null)
            {
                thread.LineNumber = (thread.DiffLineType == DiffChangeType.Delete) ?
                    line.OldLineNumber - 1 :
                    line.NewLineNumber - 1;
            }
            else
            {
                thread.LineNumber = -1;
            }
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

        static Task<string> ReadAllTextAsync(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var reader = File.OpenText(path))
                    {
                        return reader.ReadToEndAsync();
                    }
                }
                catch { }
            }

            return Task.FromResult<string>(null);
        }
    }
}
