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
using static Octokit.GraphQL.Variable;

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
        static ICompiledQuery<PullRequestDetailModel> readPullRequest;
        static ICompiledQuery<ActorModel> readViewer;

        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;
        readonly IApiClientFactory apiClientFactory;
        readonly IGraphQLClientFactory graphqlFactory;
        readonly IUsageTracker usageTracker;
        readonly IAvatarProvider avatarProvider;
        readonly IDictionary<Tuple<string, string>, string> mergeBaseCache;

        [ImportingConstructor]
        public PullRequestSessionService(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService,
            IApiClientFactory apiClientFactory,
            IGraphQLClientFactory graphqlFactory,
            IUsageTracker usageTracker,
            IAvatarProvider avatarProvider)
        {
            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
            this.apiClientFactory = apiClientFactory;
            this.graphqlFactory = graphqlFactory;
            this.usageTracker = usageTracker;
            this.avatarProvider = avatarProvider;

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
            PullRequestDetailModel pullRequest,
            string relativePath,
            IReadOnlyList<DiffChunk> diff,
            string headSha)
        {
            relativePath = relativePath.Replace("\\", "/");

            var threadsByPosition = pullRequest.Threads
                .Where(x => x.Path == relativePath && !x.IsOutdated)
                .OrderBy(x => x.Id)
                .GroupBy(x => Tuple.Create(x.OriginalCommitSha, x.OriginalPosition));
            var threads = new List<IInlineCommentThreadModel>();

            foreach (var thread in threadsByPosition)
            {
                var hunk = thread.First().DiffHunk;
                var chunks = DiffUtilities.ParseFragment(hunk);
                var chunk = chunks.Last();
                var diffLines = chunk.Lines.Reverse().Take(5).ToList();
                var firstLine = diffLines.FirstOrDefault();
                if (firstLine == null)
                {
                    log.Warning("Ignoring in-line comment in {RelativePath} with no diff line context", relativePath);
                    continue;
                }

                var inlineThread = new InlineCommentThreadModel(
                    relativePath,
                    headSha,
                    diffLines,
                    thread.SelectMany(t => t.Comments.Select(c => new InlineCommentModel
                    {
                        Comment = c,
                        Review = pullRequest.Reviews.FirstOrDefault(x => x.Comments.Contains(c)),
                    })));
                threads.Add(inlineThread);
            }

            UpdateCommentThreads(threads, diff);
            return threads;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Tuple<int, GitHub.Models.DiffSide>> UpdateCommentThreads(
            IReadOnlyList<IInlineCommentThreadModel> threads,
            IReadOnlyList<DiffChunk> diff)
        {
            var changedLines = new List<Tuple<int, GitHub.Models.DiffSide>>();

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
                    var side = thread.DiffLineType == DiffChangeType.Delete ? GitHub.Models.DiffSide.Left : GitHub.Models.DiffSide.Right;
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

        public virtual async Task<PullRequestDetailModel> ReadPullRequestDetail(HostAddress address, string owner, string name, int number)
        {
            if (readPullRequest == null)
            {
                readPullRequest = new Query()
                    .Repository(Var(nameof(owner)), Var(nameof(name)))
                    .PullRequest(Var(nameof(number)))
                    .Select(pr => new PullRequestDetailModel
                    {
                        Id = pr.Id.Value,
                        Number = number,
                        Author = new ActorModel
                        {
                            Login = pr.Author.Login,
                            AvatarUrl = pr.Author.AvatarUrl(null),
                        },
                        Title = pr.Title,
                        Body = pr.Body,
                        BaseRefSha = pr.BaseRefOid,
                        BaseRefName = pr.BaseRefName,
                        BaseRepositoryOwner = pr.Repository.Owner.Login,
                        HeadRefName = pr.HeadRefName,
                        HeadRefSha = pr.HeadRefOid,
                        HeadRepositoryOwner = pr.HeadRepositoryOwner != null ? pr.HeadRepositoryOwner.Login : null,
                        State = (PullRequestStateEnum)pr.State,
                        UpdatedAt = pr.UpdatedAt,
                        Reviews = pr.Reviews(null, null, null, null, null, null).AllPages().Select(review => new PullRequestReviewModel
                        {
                            Id = review.Id.Value,
                            Body = review.Body,
                            CommitId = review.Commit.Oid,
                            State = (GitHub.Models.PullRequestReviewState)review.State,
                            SubmittedAt = review.SubmittedAt,
                            Author = new ActorModel
                            {
                                Login = review.Author.Login,
                                AvatarUrl = review.Author.AvatarUrl(null),
                            },
                            Comments = review.Comments(null, null, null, null).AllPages().Select(comment => new CommentAdapter
                            {
                                Id = comment.Id.Value,
                                Author = new ActorModel
                                {
                                    Login = comment.Author.Login,
                                    AvatarUrl = comment.Author.AvatarUrl(null),
                                },
                                Body = comment.Body,
                                Path = comment.Path,
                                CommitSha = comment.Commit.Oid,
                                DiffHunk = comment.DiffHunk,
                                Position = comment.Position,
                                OriginalPosition = comment.OriginalPosition,
                                OriginalCommitId = comment.OriginalCommit.Oid,
                                ReplyTo = comment.ReplyTo != null ? comment.ReplyTo.Id.Value : null,
                                CreatedAt = comment.CreatedAt,
                            }).ToList(),
                        }).ToList(),
                    }).Compile();
            }

            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
                { nameof(number), number },
            };

            var connection = await graphqlFactory.CreateConnection(address);
            var result = await connection.Run(readPullRequest, vars);

            var apiClient = await apiClientFactory.Create(address);
            var files = await apiClient.GetPullRequestFiles(owner, name, number).ToList();

            result.ChangedFiles = files.Select(file => new PullRequestFileModel
            {
                FileName = file.FileName,
                Sha = file.Sha,
                Status = (PullRequestFileStatus)Enum.Parse(typeof(PullRequestFileStatus), file.Status, true),
            }).ToList();

            var commentsByReplyId = new Dictionary<string, List<CommentAdapter>>();

            foreach (CommentAdapter comment in result.Reviews.SelectMany(x => x.Comments))
            {
                if (comment.ReplyTo == null)
                {
                    commentsByReplyId.Add(comment.Id, new List<CommentAdapter> { comment });
                }
            }

            foreach (CommentAdapter comment in result.Reviews.SelectMany(x => x.Comments))
            {
                if (comment.ReplyTo != null)
                {
                    List<CommentAdapter> thread;

                    if (commentsByReplyId.TryGetValue(comment.ReplyTo, out thread))
                    {
                        thread.Add(comment);
                    }
                }
            }

            var threads = new List<PullRequestReviewThreadModel>();

            foreach (var thread in commentsByReplyId)
            {
                var c = thread.Value[0];
                threads.Add(new PullRequestReviewThreadModel
                {
                    Comments = thread.Value,
                    CommitSha = c.CommitSha,
                    DiffHunk = c.DiffHunk,
                    Id = c.Id,
                    IsOutdated = c.Position == null,
                    OriginalCommitSha = c.OriginalCommitId,
                    OriginalPosition = c.OriginalPosition,
                    Path = c.Path,
                    Position = c.Position,
                });
            }

            result.Threads = threads;
            return result;
        }

        public virtual async Task<ActorModel> ReadViewer(HostAddress address)
        {
            if (readViewer == null)
            {
                readViewer = new Query()
                    .Viewer
                    .Select(x => new ActorModel
                    {
                        Login = x.Login,
                        AvatarUrl = x.AvatarUrl(null),
                    }).Compile();
            }

            var connection = await graphqlFactory.CreateConnection(address);
            return await connection.Run(readViewer);
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

            return (await graphql.Run(query)).Value;
        }

        /// <inheritdoc/>
        public virtual async Task<string> GetPullRequestMergeBase(ILocalRepositoryModel repository, PullRequestDetailModel pullRequest)
        {
            var baseSha = pullRequest.BaseRefSha;
            var headSha = pullRequest.HeadRefSha;
            var key = new Tuple<string, string>(baseSha, headSha);

            string mergeBase;
            if (mergeBaseCache.TryGetValue(key, out mergeBase))
            {
                return mergeBase;
            }

            using (var repo = await GetRepository(repository))
            {
                var targetUrl = repository.CloneUrl.WithOwner(pullRequest.BaseRepositoryOwner);
                var headUrl = repository.CloneUrl.WithOwner(pullRequest.HeadRepositoryOwner);
                var baseRef = pullRequest.BaseRefName;
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
        public async Task<PullRequestReviewModel> CreatePendingReview(
            ILocalRepositoryModel localRepository,
            string pullRequestId)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var review = new AddPullRequestReviewInput
            {
                PullRequestId = new ID(pullRequestId),
            };

            var addReview = new Mutation()
                .AddPullRequestReview(review)
                .Select(x => new PullRequestReviewModel
                {
                    Id = x.PullRequestReview.Id.Value,
                    Body = x.PullRequestReview.Body,
                    CommitId = x.PullRequestReview.Commit.Oid,
                    State = FromGraphQL(x.PullRequestReview.State),
                    Author = new ActorModel
                    {
                        Login = x.PullRequestReview.Author.Login,
                        AvatarUrl = x.PullRequestReview.Author.AvatarUrl(null),
                    },
                });

            var result = await graphql.Run(addReview);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentStartReview);
            return result;
        }

        /// <inheritdoc/>
        public async Task CancelPendingReview(
            ILocalRepositoryModel localRepository,
            string reviewId)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var delete = new DeletePullRequestReviewInput
            {
                PullRequestReviewId = new ID(reviewId),
            };

            var deleteReview = new Mutation()
                .DeletePullRequestReview(delete)
                .Select(x => x.ClientMutationId);

            await graphql.Run(deleteReview);
        }

        /// <inheritdoc/>
        public async Task<PullRequestReviewModel> PostReview(
            ILocalRepositoryModel localRepository,
            string pullRequestId,
            string commitId,
            string body,
            PullRequestReviewEvent e)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var addReview = new AddPullRequestReviewInput
            {
                Body = body,
                CommitOID = commitId,
                Event = ToGraphQl(e),
                PullRequestId = new ID(pullRequestId),
            };

            var mutation = new Mutation()
                .AddPullRequestReview(addReview)
                .Select(review => new PullRequestReviewModel
                {
                    Id = review.PullRequestReview.Id.Value,
                    Body = body,
                    CommitId = review.PullRequestReview.Commit.Oid,
                    Comments = new PullRequestReviewCommentModel[0],
                    State = (GitHub.Models.PullRequestReviewState)review.PullRequestReview.State,
                    Author = new ActorModel
                    {
                        Login = review.PullRequestReview.Author.Login,
                        AvatarUrl = review.PullRequestReview.Author.AvatarUrl(null),
                    },
                });

            var result = await graphql.Run(mutation);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewPosts);
            return result;
        }

        public async Task<PullRequestReviewModel> SubmitPendingReview(
            ILocalRepositoryModel localRepository,
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
                PullRequestReviewId = new ID(pendingReviewId),
            };

            var mutation = new Mutation()
                .SubmitPullRequestReview(submit)
                .Select(review => new PullRequestReviewModel
                {
                    Id = review.PullRequestReview.Id.Value,
                    Body = body,
                    CommitId = review.PullRequestReview.Commit.Oid,
                    Comments = review.PullRequestReview.Comments(100, null, null, null).AllPages().Select(comment => new CommentAdapter
                    {
                        Id = comment.Id.Value,
                        Author = new ActorModel
                        {
                            Login = comment.Author.Login,
                            AvatarUrl = comment.Author.AvatarUrl(null),
                        },
                        Body = comment.Body,
                        CommitSha = comment.Commit.Oid,
                        CreatedAt = comment.CreatedAt,
                        DiffHunk = comment.DiffHunk,
                        OriginalCommitId = comment.OriginalCommit.Oid,
                        OriginalPosition = comment.OriginalPosition,
                        Path = comment.Path,
                        Position = comment.Position,
                        ReplyTo = comment.ReplyTo.Id.Value,
                    }).ToList(),
                    State = (GitHub.Models.PullRequestReviewState)review.PullRequestReview.State,
                    Author = new ActorModel
                    {
                        Login = review.PullRequestReview.Author.Login,
                        AvatarUrl = review.PullRequestReview.Author.AvatarUrl(null),
                    },
                });

            var result = await graphql.Run(mutation);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewPosts);
            return result;
        }

        /// <inheritdoc/>
        public async Task<PullRequestReviewCommentModel> PostPendingReviewComment(
            ILocalRepositoryModel localRepository,
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
                PullRequestReviewId = new ID(pendingReviewId),
            };

            var addComment = new Mutation()
                .AddPullRequestReviewComment(comment)
                .Select(x => new CommentAdapter
                {
                    Id = x.Comment.Id.Value,
                    Author = new ActorModel
                    {
                        Login = x.Comment.Author.Login,
                        AvatarUrl = x.Comment.Author.AvatarUrl(null),
                    },
                    Body = x.Comment.Body,
                    CommitSha = x.Comment.Commit.Oid,
                    Path = x.Comment.Path,
                    Position = x.Comment.Position,
                    CreatedAt = x.Comment.CreatedAt,
                    DiffHunk = x.Comment.DiffHunk,
                    OriginalPosition = x.Comment.OriginalPosition,
                    OriginalCommitId = x.Comment.OriginalCommit.Oid,
                });

            var result = await graphql.Run(addComment);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);
            return result;
        }

        /// <inheritdoc/>
        public async Task<PullRequestReviewCommentModel> PostPendingReviewCommentReply(
            ILocalRepositoryModel localRepository,
            string pendingReviewId,
            string body,
            string inReplyTo)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var comment = new AddPullRequestReviewCommentInput
            {
                Body = body,
                InReplyTo = new ID(inReplyTo),
                PullRequestReviewId = new ID(pendingReviewId),
            };

            var addComment = new Mutation()
                .AddPullRequestReviewComment(comment)
                .Select(x => new CommentAdapter
                {
                    Id = x.Comment.Id.Value,
                    Author = new ActorModel
                    {
                        Login = x.Comment.Author.Login,
                        AvatarUrl = x.Comment.Author.AvatarUrl(null),
                    },
                    Body = x.Comment.Body,
                    CommitSha = x.Comment.Commit.Oid,
                    Path = x.Comment.Path,
                    Position = x.Comment.Position,
                    CreatedAt = x.Comment.CreatedAt,
                    DiffHunk = x.Comment.DiffHunk,
                    OriginalPosition = x.Comment.OriginalPosition,
                    OriginalCommitId = x.Comment.OriginalCommit.Oid,
                });

            var result = await graphql.Run(addComment);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);
            return result;
        }

        /// <inheritdoc/>
        public async Task<PullRequestReviewModel> PostStandaloneReviewComment(
            ILocalRepositoryModel localRepository,
            string pullRequestId,
            string body,
            string commitId,
            string path,
            int position)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var addReview = new AddPullRequestReviewInput
            {
                Body = body,
                CommitOID = commitId,
                Event = Octokit.GraphQL.Model.PullRequestReviewEvent.Comment,
                PullRequestId = new ID(pullRequestId),
                Comments = new[]
                {
                    new DraftPullRequestReviewComment
                    {
                        Body = body,
                        Path = path,
                        Position = position,
                    },
                },
            };

            var mutation = new Mutation()
                .AddPullRequestReview(addReview)
                .Select(review => new PullRequestReviewModel
                {
                    Id = review.PullRequestReview.Id.Value,
                    Body = body,
                    CommitId = review.PullRequestReview.Commit.Oid,
                    Comments = review.PullRequestReview.Comments(100, null, null, null).AllPages().Select(comment => new CommentAdapter
                    {
                        Id = comment.Id.Value,
                        Author = new ActorModel
                        {
                            Login = comment.Author.Login,
                            AvatarUrl = comment.Author.AvatarUrl(null),
                        },
                        Body = comment.Body,
                        CommitSha = comment.Commit.Oid,
                        CreatedAt = comment.CreatedAt,
                        DiffHunk = comment.DiffHunk,
                        OriginalCommitId = comment.OriginalCommit.Oid,
                        OriginalPosition = comment.OriginalPosition,
                        Path = comment.Path,
                        Position = comment.Position,
                        ReplyTo = comment.ReplyTo.Id.Value,
                    }).ToList(),
                    State = (GitHub.Models.PullRequestReviewState)review.PullRequestReview.State,
                    Author = new ActorModel
                    {
                        Login = review.PullRequestReview.Author.Login,
                        AvatarUrl = review.PullRequestReview.Author.AvatarUrl(null),
                    },
                });

            var result = await graphql.Run(mutation);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);
            return result;
        }

        /// <inheritdoc/>
        public async Task<PullRequestReviewModel> PostStandaloneReviewCommentReply(
            ILocalRepositoryModel localRepository,
            string pullRequestId,
            string body,
            string inReplyTo)
        {
            var review = await CreatePendingReview(localRepository, pullRequestId);
            var comment = await PostPendingReviewCommentReply(localRepository, review.Id, body, inReplyTo);
            return await SubmitPendingReview(localRepository, review.Id, null, PullRequestReviewEvent.Comment);
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

        class CommentAdapter : PullRequestReviewCommentModel
        {
            public string Path { get; set; }
            public string CommitSha { get; set; }
            public string DiffHunk { get; set; }
            public int? Position { get; set; }
            public int OriginalPosition { get; set; }
            public string OriginalCommitId { get; set; }
            public string ReplyTo { get; set; }
        }
    }
}
