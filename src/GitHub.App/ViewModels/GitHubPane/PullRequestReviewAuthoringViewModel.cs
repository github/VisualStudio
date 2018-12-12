using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Models.Drafts;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
using Serilog;
using static System.FormattableString;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestReviewAuthoringViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestReviewAuthoringViewModel : PanePageViewModelBase, IPullRequestReviewAuthoringViewModel
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestReviewAuthoringViewModel>();

        readonly IPullRequestEditorService editorService;
        readonly IPullRequestSessionManager sessionManager;
        readonly IMessageDraftStore draftStore;
        readonly IPullRequestService pullRequestService;
        readonly IScheduler timerScheduler;
        IPullRequestSession session;
        IDisposable sessionSubscription;
        PullRequestReviewModel model;
        PullRequestDetailModel pullRequestModel;
        string body;
        ObservableAsPropertyHelper<bool> canApproveRequestChanges;
        IReadOnlyList<IPullRequestReviewFileCommentViewModel> fileComments;
        string operationError;

        [ImportingConstructor]
        public PullRequestReviewAuthoringViewModel(
            IPullRequestService pullRequestService,
            IPullRequestEditorService editorService,
            IPullRequestSessionManager sessionManager,
            IMessageDraftStore draftStore,
            IPullRequestFilesViewModel files)
            : this(pullRequestService, editorService, sessionManager,draftStore, files, DefaultScheduler.Instance)
        {
        }

        public PullRequestReviewAuthoringViewModel(
            IPullRequestService pullRequestService,
            IPullRequestEditorService editorService,
            IPullRequestSessionManager sessionManager,
            IMessageDraftStore draftStore,
            IPullRequestFilesViewModel files,
            IScheduler timerScheduler)
        {
            Guard.ArgumentNotNull(editorService, nameof(editorService));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(draftStore, nameof(draftStore));
            Guard.ArgumentNotNull(files, nameof(files));
            Guard.ArgumentNotNull(timerScheduler, nameof(timerScheduler));

            this.pullRequestService = pullRequestService;
            this.editorService = editorService;
            this.sessionManager = sessionManager;
            this.draftStore = draftStore;
            this.timerScheduler = timerScheduler;

            canApproveRequestChanges = this.WhenAnyValue(
                x => x.Model,
                x => x.PullRequestModel,
                (review, pr) => review != null && pr != null && review.Author.Login != pr.Author.Login)
                .ToProperty(this, x => x.CanApproveRequestChanges);

            Files = files;

            var hasBodyOrComments = this.WhenAnyValue(
                x => x.Body,
                x => x.FileComments.Count,
                (body, comments) => !string.IsNullOrWhiteSpace(body) || comments > 0);

            Approve = ReactiveCommand.CreateFromTask(() => DoSubmit(Octokit.PullRequestReviewEvent.Approve));
            Comment = ReactiveCommand.CreateFromTask(
                () => DoSubmit(Octokit.PullRequestReviewEvent.Comment),
                hasBodyOrComments);
            RequestChanges = ReactiveCommand.CreateFromTask(
                () => DoSubmit(Octokit.PullRequestReviewEvent.RequestChanges),
                hasBodyOrComments);
            Cancel = ReactiveCommand.CreateFromTask(DoCancel);
            NavigateToPullRequest = ReactiveCommand.Create(() =>
                NavigateTo(Invariant($"{RemoteRepositoryOwner}/{LocalRepository.Name}/pull/{PullRequestModel.Number}")));
        }

        /// <inheritdoc/>
        public LocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public string RemoteRepositoryOwner { get; private set; }

        /// <inheritdoc/>
        public PullRequestReviewModel Model
        {
            get { return model; }
            private set { this.RaiseAndSetIfChanged(ref model, value); }
        }

        /// <inheritdoc/>
        public PullRequestDetailModel PullRequestModel
        {
            get { return pullRequestModel; }
            private set { this.RaiseAndSetIfChanged(ref pullRequestModel, value); }
        }

        /// <inheritdoc/>
        public IPullRequestFilesViewModel Files { get; }

        /// <inheritdoc/>
        public string Body
        {
            get { return body; }
            set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <inheritdoc/>
        public bool CanApproveRequestChanges => canApproveRequestChanges.Value;

        /// <summary>
        /// Gets the error message to be displayed in the action area as a result of an error in a
        /// git operation.
        /// </summary>
        public string OperationError
        {
            get { return operationError; }
            private set { this.RaiseAndSetIfChanged(ref operationError, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments
        {
            get { return fileComments; }
            private set { this.RaiseAndSetIfChanged(ref fileComments, value); }
        }

        public ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }
        public ReactiveCommand<Unit, Unit> Approve { get; }
        public ReactiveCommand<Unit, Unit> Comment { get; }
        public ReactiveCommand<Unit, Unit> RequestChanges { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public async Task InitializeAsync(
            LocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber)
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
                session = await sessionManager.GetSession(owner, repo, pullRequestNumber).ConfigureAwait(true);
                await Load(session.PullRequest).ConfigureAwait(true);

                if (LocalRepository?.CloneUrl != null)
                {
                    var key = GetDraftKey();

                    if (string.IsNullOrEmpty(Body))
                    {
                        var draft = await draftStore.GetDraft<PullRequestReviewDraft>(key, string.Empty)
                            .ConfigureAwait(true);
                        Body = draft?.Body;
                    }

                    this.WhenAnyValue(x => x.Body)
                        .Throttle(TimeSpan.FromSeconds(1), timerScheduler)
                        .Select(x => new PullRequestReviewDraft { Body = x })
                        .Subscribe(x => draftStore.UpdateDraft(key, string.Empty, x));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <inheritdoc/>
        public override async Task Refresh()
        {
            try
            {
                Error = null;
                IsBusy = true;
                await session.Refresh();
                await Load(session.PullRequest);
            }
            catch (Exception ex)
            {
                log.Error(
                    ex,
                    "Error loading pull request review {Owner}/{Repo}/{Number}/{PullRequestReviewId} from {Address}",
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    PullRequestModel.Number,
                    Model.Id,
                    session.LocalRepository.CloneUrl.Host);
                Error = ex;
                IsBusy = false;
            }
        }

        public static string GetDraftKey(
            UriString cloneUri,
            int pullRequestNumber)
        {
            return Invariant($"pr-review|{cloneUri}|{pullRequestNumber}");
        }

        protected string GetDraftKey()
        {
            return GetDraftKey(
                LocalRepository.CloneUrl.WithOwner(RemoteRepositoryOwner),
                PullRequestModel.Number);
        }

        async Task Load(PullRequestDetailModel pullRequest)
        {
            try
            {
                PullRequestModel = pullRequest;

                Model = pullRequest.Reviews.FirstOrDefault(x =>
                    x.State == PullRequestReviewState.Pending && x.Author.Login == session.User.Login) ??
                    new PullRequestReviewModel
                    {
                        Body = string.Empty,
                        Author = session.User,
                        State = PullRequestReviewState.Pending,
                    };

                Body = Model.Body;

                sessionSubscription?.Dispose();
                await UpdateFileComments();
                sessionSubscription = session.PullRequestChanged.Subscribe(_ => UpdateFileComments().Forget());
            }
            finally
            {
                IsBusy = false;
            }
        }

        bool FilterComments(IInlineCommentThreadModel thread)
        {
            return thread.Comments.Any(x => x.Review.Id == Model.Id);
        }

        async Task UpdateFileComments()
        {
            var result = new List<PullRequestReviewCommentViewModel>();

            if (Model.Id == null && session.PendingReviewId != null)
            {
                Model.Id = session.PendingReviewId;
            }

            foreach (var file in await session.GetAllFiles())
            {
                foreach (var thread in file.InlineCommentThreads)
                {
                    foreach (var comment in thread.Comments)
                    {
                        if (comment.Review.Id == Model.Id)
                        {
                            result.Add(new PullRequestReviewCommentViewModel(
                                editorService,
                                session,
                                thread.RelativePath,
                                comment.Comment));
                        }
                    }
                }
            }

            FileComments = result;
            await Files.InitializeAsync(session, FilterComments);
        }

        async Task DoSubmit(Octokit.PullRequestReviewEvent e)
        {
            OperationError = null;
            IsBusy = true;

            try
            {
                await session.PostReview(Body, e).ConfigureAwait(true);
                Close();
                await draftStore.DeleteDraft(GetDraftKey(), string.Empty).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OperationError = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task DoCancel()
        {
            OperationError = null;
            IsBusy = true;

            try
            {
                if (Model?.Id != null)
                {
                    if (pullRequestService.ConfirmCancelPendingReview())
                    {
                        await session.CancelReview();
                        Close();
                    }
                }
                else
                {
                    Close();
                }

                await draftStore.DeleteDraft(GetDraftKey(), string.Empty).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                OperationError = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
