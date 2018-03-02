using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using Serilog;
using static System.FormattableString;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// View model for displaying details of a pull request review.
    /// </summary>
    [Export(typeof(IPullRequestReviewViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestReviewViewModel : PanePageViewModelBase, IPullRequestReviewViewModel
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestReviewViewModel>();

        readonly IPullRequestSessionManager sessionManager;
        readonly IModelServiceFactory modelServiceFactory;
        IModelService modelService;
        IPullRequestSession session;
        IPullRequestReviewModel model;
        IDisposable sessionSubscription;
        string title;
        string state;
        bool isPending;
        string body;
        IReadOnlyList<IPullRequestReviewCommentModel> fileComments;
        IReadOnlyList<IPullRequestReviewCommentModel> outdatedFileComments;
        int commentCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReviewViewModel"/> class.
        /// </summary>
        /// <param name="pullRequestsService">The pull requests service.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="modelServiceFactory">The model service factory.</param>
        /// <param name="files">The pull request files view model.</param>
        [ImportingConstructor]
        public PullRequestReviewViewModel(
            IPullRequestEditorService editorService,
            IPullRequestSessionManager sessionManager,
            IModelServiceFactory modelServiceFactory,
            IPullRequestFilesViewModel files)
        {
            Guard.ArgumentNotNull(editorService, nameof(editorService));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));
            Guard.ArgumentNotNull(files, nameof(files));

            this.sessionManager = sessionManager;
            this.modelServiceFactory = modelServiceFactory;

            Files = files;
            NavigateToPullRequest = ReactiveCommand.Create().OnExecuteCompleted(_ =>
                NavigateTo(Invariant($"{LocalRepository.Owner}/{LocalRepository.Name}/pull/{PullRequestNumber}")));
            Submit = ReactiveCommand.CreateAsyncTask(DoSubmit);

            OpenComment = ReactiveCommand.CreateAsyncTask(async x =>
            {
                var comment = (IPullRequestReviewCommentModel)x;
                var file = await session.GetFile(comment.Path);
                var thread = file.InlineCommentThreads.FirstOrDefault(y => y.Comments.Any(z => z.Id == comment.Id));

                if (thread != null && thread.LineNumber != -1)
                {
                    await editorService.OpenDiff(session, file.RelativePath, thread);
                }
            });
        }

        /// <inheritdoc/>
        public ILocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public string RemoteRepositoryOwner { get; private set; }

        /// <inheritdoc/>
        public int PullRequestNumber { get; private set; }

        /// <inheritdoc/>
        public long PullRequestReviewId { get; private set; }

        /// <inheritdoc/>
        public IPullRequestFilesViewModel Files { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewCommentModel> FileComments
        {
            get { return fileComments; }
            private set { this.RaiseAndSetIfChanged(ref fileComments, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewCommentModel> OutdatedFileComments
        {
            get { return outdatedFileComments; }
            private set { this.RaiseAndSetIfChanged(ref outdatedFileComments, value); }
        }

        /// <inheritdoc/>
        public int CommentCount
        {
            get { return commentCount; }
            private set { this.RaiseAndSetIfChanged(ref commentCount, value); }
        }

        /// <inheritdoc/>
        public IPullRequestReviewModel Model
        {
            get { return model; }
            private set { this.RaiseAndSetIfChanged(ref model, value); }
        }

        /// <inheritdoc/>
        public string Title
        {
            get { return title; }
            private set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        /// <inheritdoc/>
        public string State
        {
            get { return state; }
            private set { this.RaiseAndSetIfChanged(ref state, value); }
        }

        /// <inheritdoc/>
        public bool IsPending
        {
            get { return isPending; }
            private set { this.RaiseAndSetIfChanged(ref isPending, value); }
        }

        /// <inheritdoc/>
        public string Body
        {
            get { return body; }
            set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> OpenComment { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> NavigateToPullRequest { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> Submit { get; }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber,
            long pullRequestReviewId)
        {
            if (repo != localRepository.Name)
            {
                throw new NotSupportedException();
            }

            IsLoading = true;

            try
            {
                LocalRepository = localRepository;
                RemoteRepositoryOwner = owner;
                PullRequestNumber = pullRequestNumber;
                PullRequestReviewId = pullRequestReviewId;
                modelService = await modelServiceFactory.CreateAsync(connection);
                await Refresh();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <inheritdoc/>
        public Task InitializeNewAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber)
        {
            return InitializeAsync(localRepository, connection, owner, repo, pullRequestNumber, 0);
        }

        /// <inheritdoc/>
        public override async Task Refresh()
        {
            try
            {
                Error = null;
                IsBusy = true;
                var pullRequest = await modelService.GetPullRequest(RemoteRepositoryOwner, LocalRepository.Name, PullRequestNumber);
                await Load(pullRequest);
            }
            catch (Exception ex)
            {
                log.Error(
                    ex,
                    "Error loading pull request review {Owner}/{Repo}/{Number}/{PullRequestReviewId} from {Address}",
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    PullRequestNumber,
                    PullRequestReviewId,
                    modelService.ApiClient.HostAddress.Title);
                Error = ex;
                IsBusy = false;
            }
        }

        /// <inheritdoc/>
        public async Task Load(IPullRequestModel pullRequest)
        {
            try
            {
                session = await sessionManager.GetSession(pullRequest);

                if (PullRequestReviewId > 0)
                {
                    Model = pullRequest.Reviews.Single(x => x.Id == PullRequestReviewId);
                    Title = pullRequest.Title;
                    State = PullRequestDetailReviewItem.ToString(Model.State);
                    IsPending = Model.State == PullRequestReviewState.Pending;
                    Body = IsPending || !string.IsNullOrWhiteSpace(Model.Body) ? 
                        Model.Body :
                        Resources.NoDescriptionProvidedMarkdown;
                }
                else
                {
                    Model = null;
                    Title = null;
                    State = null;
                    IsPending = true;
                    Body = string.Empty;
                }

                await Files.InitializeAsync(session, FilterComments);

                sessionSubscription?.Dispose();
                sessionSubscription = session.WhenAnyValue(x => x.PullRequest.ReviewComments)
                    .Subscribe(UpdateFileComments);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Files.Dispose();
                sessionSubscription?.Dispose();
            }
        }

        async Task DoSubmit(object arg)
        {
            try
            {
                Octokit.PullRequestReviewEvent e;

                if (Enum.TryParse(arg.ToString(), out e))
                {
                    await session.PostReview(Body, e);
                    Close();
                }
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        bool FilterComments(IInlineCommentThreadModel thread)
        {
            return thread.Comments.Any(x => x.PullRequestReviewId == PullRequestReviewId);
        }

        void UpdateFileComments(IReadOnlyList<IPullRequestReviewCommentModel> comments)
        {
            var current = new List<IPullRequestReviewCommentModel>();
            var outdated = new List<IPullRequestReviewCommentModel>();

            foreach (var comment in comments)
            {
                if (comment.PullRequestReviewId == PullRequestReviewId)
                {
                    if (comment.Position.HasValue)
                    {
                        current.Add(comment);
                    }
                    else
                    {
                        outdated.Add(comment);
                    }
                }
            }

            FileComments = current;
            OutdatedFileComments = outdated;
            CommentCount = current.Count + outdated.Count;
        }
    }
}
