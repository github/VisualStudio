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
    public class PullRequestSession : ReactiveObject, IPullRequestSession
    {
        static readonly List<IPullRequestReviewCommentModel> Empty = new List<IPullRequestReviewCommentModel>();
        readonly IPullRequestSessionService service;
        readonly Dictionary<string, PullRequestSessionFile> fileIndex = new Dictionary<string, PullRequestSessionFile>();
        readonly SemaphoreSlim getFilesLock = new SemaphoreSlim(1);
        bool isCheckedOut;
        IReadOnlyList<IPullRequestSessionFile> files;

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
            User = user;
            PullRequest = pullRequest;
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
            return await GetFile(relativePath, null);
        }

        /// <inheritdoc/>
        public async Task<IPullRequestSessionFile> GetFile(
            string relativePath,
            IEditorContentSource contentSource)
        {
            await getFilesLock.WaitAsync();

            try
            {
                PullRequestSessionFile file;

                relativePath = relativePath.Replace("\\", "/");

                if (!fileIndex.TryGetValue(relativePath, out file))
                {
                    // TODO: Check for binary files.
                    file = await CreateFile(relativePath, contentSource);
                    fileIndex.Add(relativePath, file);
                }
                else if (contentSource != null && file.ContentSource != contentSource)
                {
                    file.ContentSource = contentSource;
                    await UpdateEditorContent(relativePath);
                }

                return file;
            }
            finally
            {
                getFilesLock.Release();
            }
        }

        /// <inheritdoc/>
        public string GetRelativePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                var basePath = LocalRepository.LocalPath;

                if (path.StartsWith(basePath) && path.Length > basePath.Length + 1)
                {
                    return path.Substring(basePath.Length + 1);
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IPullRequestReviewCommentModel> PostReviewComment(string body, string commitId, string path, int position)
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
        public async Task UpdateEditorContent(string relativePath)
        {
            PullRequestSessionFile file;

            relativePath = relativePath.Replace("\\", "/");

            if (fileIndex.TryGetValue(relativePath, out file))
            {
                var content = await GetFileContent(file);

                file.CommitSha = await CalculateContentCommitSha(file, content);
                var mergeBaseSha = await service.GetPullRequestMergeBase(LocalRepository, PullRequest);
                var headSha = await CalculateHeadSha();
                file.Diff = await service.Diff(LocalRepository, mergeBaseSha, headSha, relativePath, content);
                file.InlineCommentThreads = service.BuildCommentThreads(PullRequest, file.RelativePath, file.Diff);
            }
        }

        public async Task Update(IPullRequestModel pullRequest)
        {
            PullRequest = pullRequest;

            foreach (var file in this.fileIndex.Values.ToList())
            {
                await UpdateFile(file);
            }
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
            // NOTE: We must call GetPullRequestMergeBase before GetFileContent.
            var mergeBaseSha = await service.GetPullRequestMergeBase(LocalRepository, PullRequest);
            var headSha = await CalculateHeadSha();
            var content = await GetFileContent(file);

            file.BaseSha = PullRequest.Base.Sha;
            file.CommitSha = await CalculateContentCommitSha(file, content);
            file.Diff = await service.Diff(LocalRepository, mergeBaseSha, headSha, file.RelativePath, content);
            file.InlineCommentThreads = service.BuildCommentThreads(PullRequest, file.RelativePath, file.Diff);
        }

        async Task<PullRequestSessionFile> CreateFile(
            string relativePath,
            IEditorContentSource contentSource)
        {
            var file = new PullRequestSessionFile(relativePath);
            file.ContentSource = contentSource;
            await UpdateFile(file);
            return file;
        }

        async Task<IReadOnlyList<IPullRequestSessionFile>> CreateAllFiles()
        {
            var result = new List<IPullRequestSessionFile>();

            foreach (var path in FilePaths)
            {
                result.Add(await CreateFile(path, null));
            }

            return result;
        }

        async Task<string> CalculateContentCommitSha(IPullRequestSessionFile file, byte[] content)
        {
            if (IsCheckedOut)
            {
                return await service.IsUnmodifiedAndPushed(LocalRepository, file.RelativePath, content) ?
                        await service.GetTipSha(LocalRepository) : null;
            }
            else
            {
                return PullRequest.Head.Sha;
            }       
        }

        async Task<string> CalculateHeadSha()
        {
            return IsCheckedOut ? 
                await service.GetTipSha(LocalRepository) :
                PullRequest.Head.Sha;
        }

        Task<byte[]> GetFileContent(IPullRequestSessionFile file)
        {
            if (!IsCheckedOut)
            {
                return service.ExtractFileFromGit(
                    LocalRepository,
                    PullRequest.Number,
                    PullRequest.Head.Sha,
                    file.RelativePath);
            }
            else if (file.ContentSource != null)
            {
                return file.ContentSource?.GetContent();
            }
            else
            {
                return service.ReadFileAsync(Path.Combine(LocalRepository.LocalPath, file.RelativePath));
            }
        }

        string GetFullPath(string relativePath)
        {
            return Path.Combine(LocalRepository.LocalPath, relativePath);
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
        public IPullRequestModel PullRequest { get; private set; }

        /// <inheritdoc/>
        public ILocalRepositoryModel LocalRepository { get; }

        /// <inheritdoc/>
        public string RepositoryOwner { get; }

        IEnumerable<string> FilePaths
        {
            get { return PullRequest.ChangedFiles.Select(x => x.FileName); }
        }
    }
}
