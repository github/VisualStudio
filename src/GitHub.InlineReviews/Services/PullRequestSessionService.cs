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
using DraftPullRequestReviewComment = Octokit.GraphQL.Model.DraftPullRequestReviewComment;
using FileMode = System.IO.FileMode;
using NotFoundException = LibGit2Sharp.NotFoundException;

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
        static ICompiledQuery<IEnumerable<LastCommitAdapter>> readCommitStatuses;
        static ICompiledQuery<IEnumerable<LastCommitAdapter>> readCommitStatusesEnterprise;
        static ICompiledQuery<ActorModel> readViewer;

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
        public virtual async Task<IReadOnlyList<DiffChunk>> Diff(LocalRepositoryModel repository, string baseSha, string headSha, string relativePath)
        {
            using (var repo = await GetRepository(repository))
            {
                return await diffService.Diff(repo, baseSha, headSha, relativePath);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IReadOnlyList<DiffChunk>> Diff(LocalRepositoryModel repository, string baseSha, string headSha, string relativePath, byte[] contents)
        {
            using (var repo = await GetRepository(repository))
            {
                return await diffService.Diff(repo, baseSha, headSha, relativePath, contents);
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<InlineAnnotationModel> BuildAnnotations(
            PullRequestDetailModel pullRequest,
            string relativePath)
        {
            relativePath = relativePath.Replace("\\", "/");

            return pullRequest.CheckSuites
                ?.SelectMany(checkSuite => checkSuite.CheckRuns.Select(checkRun => new { checkSuite, checkRun}))
                .SelectMany(arg =>
                    arg.checkRun.Annotations
                        .Where(annotation => annotation.Path == relativePath)
                        .Select(annotation => new InlineAnnotationModel(arg.checkSuite, arg.checkRun, annotation)))
                .OrderBy(tuple => tuple.StartLine)
                .ToArray();
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
        public virtual async Task<string> GetTipSha(LocalRepositoryModel repository)
        {
            using (var repo = await GetRepository(repository))
            {
                return repo.Head.Tip.Sha;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsUnmodifiedAndPushed(LocalRepositoryModel repository, string relativePath, byte[] contents)
        {
            using (var repo = await GetRepository(repository))
            {
                var modified = await gitClient.IsModified(repo, relativePath, contents);
                var pushed = await gitClient.IsHeadPushed(repo);

                return !modified && pushed;
            }
        }

        public async Task<byte[]> ExtractFileFromGit(
            LocalRepositoryModel repository,
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
                        Number = pr.Number,
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
                        State = pr.State.FromGraphQl(),
                        UpdatedAt = pr.UpdatedAt,
                        Reviews = pr.Reviews(null, null, null, null, null, null).AllPages().Select(review => new PullRequestReviewModel
                        {
                            Id = review.Id.Value,
                            Body = review.Body,
                            CommitId = review.Commit.Oid,
                            State = review.State.FromGraphQl(),
                            SubmittedAt = review.SubmittedAt,
                            Author = new ActorModel
                            {
                                Login = review.Author.Login,
                                AvatarUrl = review.Author.AvatarUrl(null),
                            },
                            Comments = review.Comments(null, null, null, null).AllPages().Select(comment => new CommentAdapter
                            {
                                Id = comment.Id.Value,
                                PullRequestId = comment.PullRequest.Number,
                                DatabaseId = comment.DatabaseId.Value,
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
                                Url = comment.Url,
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
            var lastCommitModel = await GetPullRequestLastCommitAdapter(address, owner, name, number);

            result.Statuses = lastCommitModel.Statuses;
            result.CheckSuites = lastCommitModel.CheckSuites;
            foreach (var checkSuite in result.CheckSuites)
            {
                checkSuite.HeadSha = lastCommitModel.HeadSha;
            }

            result.ChangedFiles = files.Select(file => new PullRequestFileModel
            {
                FileName = file.FileName,
                Sha = file.Sha,
                Status = (PullRequestFileStatus)Enum.Parse(typeof(PullRequestFileStatus), file.Status, true),
            }).ToList();

            BuildPullRequestThreads(result);
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
            LocalRepositoryModel localRepository,
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
        public virtual async Task<string> GetPullRequestMergeBase(LocalRepositoryModel repository, PullRequestDetailModel pullRequest)
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
            return Subject.Create<ITextSnapshot>(input, output);
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> CreatePendingReview(
            LocalRepositoryModel localRepository,
            string pullRequestId)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);
            var (_, owner, number) = await CreatePendingReviewCore(localRepository, pullRequestId);
            var detail = await ReadPullRequestDetail(address, owner, localRepository.Name, number);

            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentStartReview);

            return detail;
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> CancelPendingReview(
            LocalRepositoryModel localRepository,
            string reviewId)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var delete = new DeletePullRequestReviewInput
            {
                PullRequestReviewId = new ID(reviewId),
            };

            var mutation = new Mutation()
                .DeletePullRequestReview(delete)
                .Select(x => new
                {
                    x.PullRequestReview.Repository.Owner.Login,
                    x.PullRequestReview.PullRequest.Number
                });

            var result = await graphql.Run(mutation);
            return await ReadPullRequestDetail(address, result.Login, localRepository.Name, result.Number);
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> PostReview(
            LocalRepositoryModel localRepository,
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
                .Select(x => new
                {
                    x.PullRequestReview.Repository.Owner.Login,
                    x.PullRequestReview.PullRequest.Number
                });

            var result = await graphql.Run(mutation);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewPosts);
            return await ReadPullRequestDetail(address, result.Login, localRepository.Name, result.Number);
        }

        public async Task<PullRequestDetailModel> SubmitPendingReview(
            LocalRepositoryModel localRepository,
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
                .Select(x => new
                {
                    x.PullRequestReview.Repository.Owner.Login,
                    x.PullRequestReview.PullRequest.Number
                });

            var result = await graphql.Run(mutation);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewPosts);
            return await ReadPullRequestDetail(address, result.Login, localRepository.Name, result.Number);
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> PostPendingReviewComment(
            LocalRepositoryModel localRepository,
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
                .Select(x => new
                {
                    x.Comment.Repository.Owner.Login,
                    x.Comment.PullRequest.Number
                });

            var result = await graphql.Run(addComment);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);
            return await ReadPullRequestDetail(address, result.Login, localRepository.Name, result.Number);
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> PostPendingReviewCommentReply(
            LocalRepositoryModel localRepository,
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
                .Select(x => new
                {
                    x.Comment.Repository.Owner.Login,
                    x.Comment.PullRequest.Number
                });

            var result = await graphql.Run(addComment);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);
            return await ReadPullRequestDetail(address, result.Login, localRepository.Name, result.Number);
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> PostStandaloneReviewComment(
            LocalRepositoryModel localRepository,
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
                .Select(x => new
                {
                    x.PullRequestReview.Repository.Owner.Login,
                    x.PullRequestReview.PullRequest.Number
                });

            var result = await graphql.Run(mutation);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);
            return await ReadPullRequestDetail(address, result.Login, localRepository.Name, result.Number);
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> PostStandaloneReviewCommentReply(
            LocalRepositoryModel localRepository,
            string pullRequestId,
            string body,
            string inReplyTo)
        {
            var (id, _, _) = await CreatePendingReviewCore(localRepository, pullRequestId);
            var comment = await PostPendingReviewCommentReply(localRepository, id, body, inReplyTo);
            return await SubmitPendingReview(localRepository, id, null, PullRequestReviewEvent.Comment);
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> DeleteComment(
            LocalRepositoryModel localRepository,
            string remoteRepositoryOwner,
            int pullRequestId,
            int commentDatabaseId)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var apiClient = await apiClientFactory.Create(address);

            await apiClient.DeletePullRequestReviewComment(
                remoteRepositoryOwner,
                localRepository.Name,
                commentDatabaseId);

            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentDelete);
            return await ReadPullRequestDetail(address, remoteRepositoryOwner, localRepository.Name, pullRequestId);
        }

        /// <inheritdoc/>
        public async Task<PullRequestDetailModel> EditComment(LocalRepositoryModel localRepository,
            string remoteRepositoryOwner,
            string commentNodeId,
            string body)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var updatePullRequestReviewCommentInput = new UpdatePullRequestReviewCommentInput
            {
                Body = body,
                PullRequestReviewCommentId = new ID(commentNodeId),
            };

            var editComment = new Mutation().UpdatePullRequestReviewComment(updatePullRequestReviewCommentInput)
                .Select(x => new
                {
                    x.PullRequestReviewComment.Repository.Owner.Login,
                    x.PullRequestReviewComment.PullRequest.Number
                });

            var result = await graphql.Run(editComment);
            await usageTracker.IncrementCounter(x => x.NumberOfPRReviewDiffViewInlineCommentPost);
            return await ReadPullRequestDetail(address, result.Login, localRepository.Name, result.Number);
        }

        async Task<(string id, string owner, int number)> CreatePendingReviewCore(LocalRepositoryModel localRepository, string pullRequestId)
        {
            var address = HostAddress.Create(localRepository.CloneUrl.Host);
            var graphql = await graphqlFactory.CreateConnection(address);

            var input = new AddPullRequestReviewInput
            {
                PullRequestId = new ID(pullRequestId),
            };

            var mutation = new Mutation()
                .AddPullRequestReview(input)
                .Select(x => new
                {
                    Id = x.PullRequestReview.Id.Value,
                    Owner = x.PullRequestReview.Repository.Owner.Login,
                    x.PullRequestReview.PullRequest.Number
                });

            var result = await graphql.Run(mutation);
            return (result.Id, result.Owner, result.Number);
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

        Task<IRepository> GetRepository(LocalRepositoryModel repository)
        {
            return Task.Factory.StartNew(() => gitService.GetRepository(repository.LocalPath));
        }

        async Task<LastCommitAdapter> GetPullRequestLastCommitAdapter(HostAddress address, string owner, string name, int number)
        {
            ICompiledQuery<IEnumerable<LastCommitAdapter>> query;
            if (address.IsGitHubDotCom())
            {
                if (readCommitStatuses == null)
                {
                    readCommitStatuses = new Query()
                          .Repository(Var(nameof(owner)), Var(nameof(name)))
                          .PullRequest(Var(nameof(number))).Commits(last: 1).Nodes.Select(
                              commit => new LastCommitAdapter
                              {
                                  HeadSha = commit.Commit.Oid,
                                  CheckSuites = commit.Commit.CheckSuites(null, null, null, null, null).AllPages(10)
                                      .Select(suite => new CheckSuiteModel
                                      {
                                          CheckRuns = suite.CheckRuns(null, null, null, null, null).AllPages(10)
                                              .Select(run => new CheckRunModel
                                              {
                                                  Id = run.Id.Value,
                                                  Conclusion = run.Conclusion.FromGraphQl(),
                                                  Status = run.Status.FromGraphQl(),
                                                  Name = run.Name,
                                                  DetailsUrl = run.Permalink,
                                                  Summary = run.Summary,
                                                  Text = run.Text,
                                                  Annotations = run.Annotations(null, null, null, null).AllPages()
                                                      .Select(annotation => new CheckRunAnnotationModel
                                                      {
                                                          Title = annotation.Title,
                                                          Message = annotation.Message,
                                                          Path = annotation.Path,
                                                          AnnotationLevel = annotation.AnnotationLevel.Value.FromGraphQl(),
                                                          StartLine = annotation.Location.Start.Line,
                                                          EndLine = annotation.Location.End.Line,
                                                      }).ToList()
                                              }).ToList(),
                                          ApplicationName = suite.App != null ? suite.App.Name : "Private App"
                                      }).ToList(),
                                  Statuses = commit.Commit.Status
                                      .Select(context =>
                                          context.Contexts.Select(statusContext => new StatusModel
                                          {
                                              State = statusContext.State.FromGraphQl(),
                                              Context = statusContext.Context,
                                              TargetUrl = statusContext.TargetUrl,
                                              Description = statusContext.Description
                                          }).ToList()
                                      ).SingleOrDefault()
                              }
                          ).Compile();
                }

                query = readCommitStatuses;
            }
            else
            {
                if (readCommitStatusesEnterprise == null)
                {
                    readCommitStatusesEnterprise = new Query()
                     .Repository(Var(nameof(owner)), Var(nameof(name)))
                     .PullRequest(Var(nameof(number))).Commits(last: 1).Nodes.Select(
                         commit => new LastCommitAdapter
                         {
                             Statuses = commit.Commit.Status
                                 .Select(context =>
                                     context.Contexts.Select(statusContext => new StatusModel
                                     {
                                         State = statusContext.State.FromGraphQl(),
                                         Context = statusContext.Context,
                                         TargetUrl = statusContext.TargetUrl,
                                         Description = statusContext.Description,
                                     }).ToList()
                                 ).SingleOrDefault()
                         }
                     ).Compile();
                }

                query = readCommitStatusesEnterprise;
            }

            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
                { nameof(number), number },
            };

            var connection = await graphqlFactory.CreateConnection(address);
            var result = await connection.Run(query, vars);
            return result.First();
        }

        static void BuildPullRequestThreads(PullRequestDetailModel model)
        {
            var commentsByReplyId = new Dictionary<string, List<CommentAdapter>>();
           
            // Get all comments that are not replies.
            foreach (CommentAdapter comment in model.Reviews.SelectMany(x => x.Comments))
            {
                if (comment.ReplyTo == null)
                {
                    commentsByReplyId.Add(comment.Id, new List<CommentAdapter> { comment });
                }
            }

            // Get the comments that are replies and place them into the relevant list.
            foreach (CommentAdapter comment in model.Reviews.SelectMany(x => x.Comments).OrderBy(x => x.CreatedAt))
            {
                if (comment.ReplyTo != null)
                {
                    List<CommentAdapter> thread = null;

                    if (commentsByReplyId.TryGetValue(comment.ReplyTo, out thread))
                    {
                        thread.Add(comment);
                    }
                }
            }

            // Build a collection of threads for the information collected above.
            var threads = new List<PullRequestReviewThreadModel>();

            foreach (var threadSource in commentsByReplyId)
            {
                var adapter = threadSource.Value[0];

                var thread = new PullRequestReviewThreadModel
                {
                    Comments = threadSource.Value,
                    CommitSha = adapter.CommitSha,
                    DiffHunk = adapter.DiffHunk,
                    Id = adapter.Id,
                    IsOutdated = adapter.Position == null,
                    OriginalCommitSha = adapter.OriginalCommitId,
                    OriginalPosition = adapter.OriginalPosition,
                    Path = adapter.Path,
                    Position = adapter.Position,
                };

                // Set a reference to the thread in the comment.
                foreach (var comment in threadSource.Value)
                {
                    comment.Thread = thread;
                }

                threads.Add(thread);
            }

            model.Threads = threads;
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

        class LastCommitAdapter
        {
            public List<CheckSuiteModel> CheckSuites { get; set; }

            public List<StatusModel> Statuses { get; set; }

            public string HeadSha { get; set; }
        }
    }   
}
