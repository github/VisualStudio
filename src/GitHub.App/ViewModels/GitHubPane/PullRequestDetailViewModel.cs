using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Helpers;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio.StaticReviews.Contracts;
using ReactiveUI;
using ReactiveUI.Legacy;
using Serilog;
using static System.FormattableString;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace GitHub.ViewModels.GitHubPane
{
    /// <inheritdoc cref="IPullRequestDetailViewModel"/>
    [Export(typeof(IPullRequestDetailViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class PullRequestDetailViewModel : PanePageViewModelBase, IPullRequestDetailViewModel, IStaticReviewFileMap
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestDetailViewModel>();

        readonly IModelServiceFactory modelServiceFactory;
        readonly IPullRequestService pullRequestsService;
        readonly IPullRequestSessionManager sessionManager;
        readonly IUsageTracker usageTracker;
        readonly ITeamExplorerContext teamExplorerContext;
        readonly ISyncSubmodulesCommand syncSubmodulesCommand;
        readonly IViewViewModelFactory viewViewModelFactory;
        readonly IGitService gitService;

        IModelService modelService;
        PullRequestDetailModel model;
        IActorViewModel author;
        string sourceBranchDisplayName;
        string targetBranchDisplayName;
        string body;
        IReadOnlyList<IPullRequestReviewSummaryViewModel> reviews;
        IPullRequestCheckoutState checkoutState;
        IPullRequestUpdateState updateState;
        string operationError;
        bool isCheckedOut;
        bool isFromFork;
        bool isInCheckout;
        bool active;
        bool refreshOnActivate;
        Uri webUrl;
        IDisposable sessionSubscription;
        IReadOnlyList<IPullRequestCheckViewModel> checks = Array.Empty<IPullRequestCheckViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDetailViewModel"/> class.
        /// </summary>
        /// <param name="pullRequestsService">The pull requests service.</param>
        /// <param name="sessionManager">The pull request session manager.</param>
        /// <param name="modelServiceFactory">The model service factory</param>
        /// <param name="usageTracker">The usage tracker.</param>
        /// <param name="teamExplorerContext">The context for tracking repo changes</param>
        /// <param name="files">The view model which will display the changed files</param>
        /// <param name="syncSubmodulesCommand">A command that will be run when <see cref="SyncSubmodules"/> is executed</param>
        [ImportingConstructor]
        public PullRequestDetailViewModel(
            IPullRequestService pullRequestsService,
            IPullRequestSessionManager sessionManager,
            IModelServiceFactory modelServiceFactory,
            IUsageTracker usageTracker,
            ITeamExplorerContext teamExplorerContext,
            IPullRequestFilesViewModel files,
            ISyncSubmodulesCommand syncSubmodulesCommand,
            IViewViewModelFactory viewViewModelFactory,
            IGitService gitService)
        {
            Guard.ArgumentNotNull(pullRequestsService, nameof(pullRequestsService));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));
            Guard.ArgumentNotNull(teamExplorerContext, nameof(teamExplorerContext));
            Guard.ArgumentNotNull(syncSubmodulesCommand, nameof(syncSubmodulesCommand));
            Guard.ArgumentNotNull(viewViewModelFactory, nameof(viewViewModelFactory));
            Guard.ArgumentNotNull(gitService, nameof(gitService));

            this.pullRequestsService = pullRequestsService;
            this.sessionManager = sessionManager;
            this.modelServiceFactory = modelServiceFactory;
            this.usageTracker = usageTracker;
            this.teamExplorerContext = teamExplorerContext;
            this.syncSubmodulesCommand = syncSubmodulesCommand;
            this.viewViewModelFactory = viewViewModelFactory;
            this.gitService = gitService;

            Files = files;

            Checkout = ReactiveCommand.CreateFromObservable(
                DoCheckout,
                this.WhenAnyValue(x => x.CheckoutState)
                    .Cast<CheckoutCommandState>()
                    .Select(x => x != null && x.IsEnabled));
            Checkout.IsExecuting.Subscribe(x => isInCheckout = x);
            SubscribeOperationError(Checkout);

            Pull = ReactiveCommand.CreateFromObservable(
                DoPull,
                this.WhenAnyValue(x => x.UpdateState)
                    .Cast<UpdateCommandState>()
                    .Select(x => x != null && x.PullEnabled));
            SubscribeOperationError(Pull);

            Push = ReactiveCommand.CreateFromObservable(
                DoPush,
                this.WhenAnyValue(x => x.UpdateState)
                    .Cast<UpdateCommandState>()
                    .Select(x => x != null && x.PushEnabled));
            SubscribeOperationError(Push);

            SyncSubmodules = ReactiveCommand.CreateFromTask(
                DoSyncSubmodules,
                this.WhenAnyValue(x => x.UpdateState)
                    .Cast<UpdateCommandState>()
                    .Select(x => x != null && x.SyncSubmodulesEnabled));
            SyncSubmodules.Subscribe(_ => Refresh().ToObservable());
            SubscribeOperationError(SyncSubmodules);

            OpenOnGitHub = ReactiveCommand.Create(DoOpenDetailsUrl);

            ShowReview = ReactiveCommand.Create<IPullRequestReviewSummaryViewModel>(DoShowReview);

            ShowAnnotations = ReactiveCommand.Create<IPullRequestCheckViewModel>(DoShowAnnotations);
        }

        [Import(AllowDefault = true)]
        private IStaticReviewFileMapManager StaticReviewFileMapManager { get; set; }

        private void DoOpenDetailsUrl()
        {
            usageTracker.IncrementCounter(measuresModel => measuresModel.NumberOfPRDetailsOpenInGitHub).Forget();
        }

        /// <inheritdoc/>
        public PullRequestDetailModel Model
        {
            get { return model; }
            private set
            {
                // PullRequestModel overrides Equals such that two PRs with the same number are
                // considered equal. This was causing the Model not to be updated on refresh:
                // we need to use ReferenceEquals.
                if (!ReferenceEquals(model, value))
                {
                    this.RaisePropertyChanging(nameof(Model));
                    model = value;
                    this.RaisePropertyChanged(nameof(Model));
                }
            }
        }

        /// <inheritdoc/>
        public LocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public string RemoteRepositoryOwner { get; private set; }

        /// <inheritdoc/>
        public int Number { get; private set; }

        /// <inheritdoc/>
        public IActorViewModel Author
        {
            get { return author; }
            private set { this.RaiseAndSetIfChanged(ref author, value); }
        }

        /// <inheritdoc/>
        public IPullRequestSession Session { get; private set; }

        /// <inheritdoc/>
        public string SourceBranchDisplayName
        {
            get { return sourceBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref sourceBranchDisplayName, value); }
        }

        /// <inheritdoc/>
        public string TargetBranchDisplayName
        {
            get { return targetBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref targetBranchDisplayName, value); }
        }

        /// <inheritdoc/>
        public bool IsCheckedOut
        {
            get { return isCheckedOut; }
            private set { this.RaiseAndSetIfChanged(ref isCheckedOut, value); }
        }

        /// <inheritdoc/>
        public bool IsFromFork
        {
            get { return isFromFork; }
            private set { this.RaiseAndSetIfChanged(ref isFromFork, value); }
        }

        /// <inheritdoc/>
        public string Body
        {
            get { return body; }
            private set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <inheritdoc/>
        public IPullRequestCheckoutState CheckoutState
        {
            get { return checkoutState; }
            private set { this.RaiseAndSetIfChanged(ref checkoutState, value); }
        }

        /// <inheritdoc/>
        public IPullRequestUpdateState UpdateState
        {
            get { return updateState; }
            private set { this.RaiseAndSetIfChanged(ref updateState, value); }
        }

        /// <inheritdoc/>
        public string OperationError
        {
            get { return operationError; }
            private set { this.RaiseAndSetIfChanged(ref operationError, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewSummaryViewModel> Reviews
        {
            get { return reviews; }
            private set { this.RaiseAndSetIfChanged(ref reviews, value); }
        }

        /// <inheritdoc/>
        public IPullRequestFilesViewModel Files { get; }

        /// <summary>
        /// Gets the web URL for the pull request.
        /// </summary>
        public Uri WebUrl
        {
            get { return webUrl; }
            private set { this.RaiseAndSetIfChanged(ref webUrl, value); }
        }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> Checkout { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> Pull { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> Push { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> SyncSubmodules { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> OpenOnGitHub { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestReviewSummaryViewModel, Unit> ShowReview { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestCheckViewModel, Unit> ShowAnnotations { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestCheckViewModel> Checks
        {
            get { return checks; }
            private set { this.RaiseAndSetIfChanged(ref checks, value); }
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            LocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int number)
        {
            IsLoading = true;

            try
            {
                if (!string.Equals(repo, localRepository.Name, StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException("Showing pull requests from other repositories not yet supported.");
                }

                LocalRepository = localRepository;
                RemoteRepositoryOwner = owner;
                Number = number;
                WebUrl = localRepository.CloneUrl.ToRepositoryUrl(owner).Append("pull/" + number);
                modelService = await modelServiceFactory.CreateAsync(connection);
                Session = await sessionManager.GetSession(owner, repo, number);
                await Load(Session.PullRequest);
                teamExplorerContext.StatusChanged += RefreshIfActive;
            }
            catch (Exception ex)
            {
                Error = ex;
            }
            finally
            {
                IsLoading = false;
            }
        }

        void RefreshIfActive(object sender, EventArgs e)
        {
            if (active)
            {
                Refresh().Forget();
            }
            else
            {
                refreshOnActivate = true;
            }
        }

        /// <summary>
        /// Loads the view model from octokit models.
        /// </summary>
        /// <param name="pullRequest">The pull request model.</param>
        public async Task Load(PullRequestDetailModel pullRequest)
        {
            try
            {
                var firstLoad = (Model == null);
                Model = pullRequest;
                Author = new ActorViewModel(pullRequest.Author);
                Title = Resources.PullRequestNavigationItemText + " #" + pullRequest.Number;

                IsBusy = true;
                IsFromFork = !pullRequestsService.IsPullRequestFromRepository(LocalRepository, pullRequest);
                SourceBranchDisplayName = GetBranchDisplayName(IsFromFork, pullRequest.HeadRepositoryOwner, pullRequest.HeadRefName);
                TargetBranchDisplayName = GetBranchDisplayName(IsFromFork, pullRequest.BaseRepositoryOwner, pullRequest.BaseRefName);
                Body = !string.IsNullOrWhiteSpace(pullRequest.Body) ? pullRequest.Body : Resources.NoDescriptionProvidedMarkdown;
                Reviews = PullRequestReviewSummaryViewModel.BuildByUser(Session.User, pullRequest).ToList();

                Checks = (IReadOnlyList<IPullRequestCheckViewModel>)PullRequestCheckViewModel.Build(viewViewModelFactory, pullRequest)?.ToList() ?? Array.Empty<IPullRequestCheckViewModel>();

                await Files.InitializeAsync(Session);

                var localBranches = await pullRequestsService.GetLocalBranches(LocalRepository, pullRequest).ToList();

                var currentBranch = gitService.GetBranch(LocalRepository);
                IsCheckedOut = localBranches.Contains(currentBranch);

                if (IsCheckedOut)
                {
                    var divergence = await pullRequestsService.CalculateHistoryDivergence(LocalRepository, Model.Number);
                    var pullEnabled = divergence.BehindBy > 0;
                    var pushEnabled = divergence.AheadBy > 0 && !pullEnabled;
                    string pullToolTip;
                    string pushToolTip;

                    if (pullEnabled)
                    {
                        pullToolTip = string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.PullRequestDetailsPullToolTip,
                            IsFromFork ? Resources.Fork : Resources.Remote,
                            SourceBranchDisplayName);
                    }
                    else
                    {
                        pullToolTip = Resources.NoCommitsToPull;
                    }

                    if (pushEnabled)
                    {
                        pushToolTip = string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.PullRequestDetailsPushToolTip,
                            IsFromFork ? Resources.Fork : Resources.Remote,
                            SourceBranchDisplayName);
                    }
                    else if (divergence.AheadBy == 0)
                    {
                        pushToolTip = Resources.NoCommitsToPush;
                    }
                    else
                    {
                        pushToolTip = Resources.MustPullBeforePush;
                    }

                    var submodulesToSync = await pullRequestsService.CountSubmodulesToSync(LocalRepository);
                    var syncSubmodulesToolTip = string.Format(CultureInfo.InvariantCulture, Resources.SyncSubmodules, submodulesToSync);

                    UpdateState = new UpdateCommandState(divergence, pullEnabled, pushEnabled, pullToolTip, pushToolTip, syncSubmodulesToolTip, submodulesToSync);
                    CheckoutState = null;
                }
                else
                {
                    var caption = localBranches.Count > 0 ?
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.PullRequestDetailsCheckout,
                            localBranches.First().DisplayName) :
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.PullRequestDetailsCheckoutTo,
                            await pullRequestsService.GetDefaultLocalBranchName(LocalRepository, Model.Number, Model.Title));
                    var clean = await pullRequestsService.IsWorkingDirectoryClean(LocalRepository);
                    string disabled = null;

                    if (pullRequest.HeadRepositoryOwner == null)
                    {
                        disabled = Resources.SourceRepositoryNoLongerAvailable;
                    }
                    else if (!clean)
                    {
                        disabled = Resources.WorkingDirectoryHasUncommittedCHanges;
                    }

                    CheckoutState = new CheckoutCommandState(caption, disabled);
                    UpdateState = null;
                }

                sessionSubscription?.Dispose();
                sessionSubscription = Session.WhenAnyValue(x => x.HasPendingReview)
                    .Skip(1)
                    .Subscribe(x => Reviews = PullRequestReviewSummaryViewModel.BuildByUser(Session.User, Session.PullRequest).ToList());

                if (firstLoad)
                {
                    usageTracker.IncrementCounter(x => x.NumberOfPullRequestsOpened).Forget();
                }

                if (!isInCheckout)
                {
                    pullRequestsService.RemoveUnusedRemotes(LocalRepository).Subscribe(_ => { });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Refreshes the contents of the view model.
        /// </summary>
        public override async Task Refresh()
        {
            try
            {
                await ThreadingHelper.SwitchToMainThreadAsync();

                Error = null;
                OperationError = null;
                IsBusy = true;
                await Session.Refresh();
                await Load(Session.PullRequest);
            }
            catch (Exception ex)
            {
                log.Error(
                    ex,
                    "Error loading pull request {Owner}/{Repo}/{Number} from {Address}",
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    Number,
                    modelService.ApiClient.HostAddress.Title);
                Error = ex;
                IsBusy = false;
            }
        }

        /// <inheritdoc/>
        public string GetLocalFilePath(IPullRequestFileNode file)
        {
            return Path.Combine(LocalRepository.LocalPath, file.RelativePath);
        }

        /// <inheritdoc/>
        public override void Activated()
        {
            active = true;
            this.StaticReviewFileMapManager?.RegisterStaticReviewFileMap(this);

            if (refreshOnActivate)
            {
                Refresh().Forget();
                refreshOnActivate = false;
            }
        }

        /// <inheritdoc/>
        public override void Deactivated()
        {
            this.StaticReviewFileMapManager?.UnregisterStaticReviewFileMap(this);
            active = false;
        }

        /// <inheritdoc/>
        public Task<string> GetLocalPathFromObjectishAsync(string objectish, CancellationToken cancellationToken)
        {
            if (this.pullRequestsService != null)
            {
                string commitId = objectish.Substring(0, objectish.IndexOf(':'));
                string relativePath = objectish.Substring(objectish.IndexOf(':')+1).TrimStart('/');

                return this.pullRequestsService.ExtractToTempFile(
                    this.Session.LocalRepository,
                    this.Session.PullRequest,
                    relativePath,
                    commitId,
                    this.pullRequestsService.GetEncoding(this.Session.LocalRepository, relativePath));
            }

            return Task.FromResult<string>(null);
        }

        /// <inheritdoc/>
        public Task<string> GetObjectishFromLocalPathAsync(string localPath, CancellationToken cancellationToken)
        {
            // We rely on pull request service's global map here instead of trying to get it from IPullRequestSessionManager via ITextBuffer
            // because it is possible that the file queried wasn't opened by GitHub extension and instead was opened by LSP
            if (this.pullRequestsService is IStaticReviewFileMap staticReviewFileMap)
            {
                return staticReviewFileMap.GetObjectishFromLocalPathAsync(localPath, cancellationToken);
            }

            return Task.FromResult<string>(null);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                teamExplorerContext.StatusChanged -= RefreshIfActive;
            }
        }

        void SubscribeOperationError(ReactiveCommand<Unit, Unit> command)
        {
            command.ThrownExceptions.Subscribe(x => OperationError = x.Message);
        }

        static string GetBranchDisplayName(bool isFromFork, string owner, string label)
        {
            if (owner != null)
            {
                return isFromFork ? owner + ':' + label : label;
            }
            else
            {
                return Resources.InvalidBranchName;
            }
        }

        IObservable<Unit> DoCheckout()
        {
            OperationError = null;

            return Observable.Defer(async () =>
            {
                var localBranches = await pullRequestsService.GetLocalBranches(LocalRepository, Model).ToList();

                if (localBranches.Count > 0)
                {
                    return pullRequestsService.SwitchToBranch(LocalRepository, Model);
                }
                else
                {
                    return pullRequestsService
                        .GetDefaultLocalBranchName(LocalRepository, Model.Number, Model.Title)
                        .SelectMany(x => pullRequestsService.Checkout(LocalRepository, Model, x));
                }
            }).Do(_ =>
            {
                if (IsFromFork)
                    usageTracker.IncrementCounter(x => x.NumberOfForkPullRequestsCheckedOut).Forget();
                else
                    usageTracker.IncrementCounter(x => x.NumberOfLocalPullRequestsCheckedOut).Forget();
            });
        }

        IObservable<Unit> DoPull()
        {
            OperationError = null;

            return pullRequestsService.Pull(LocalRepository)
                .Do(_ =>
                {
                    if (IsFromFork)
                        usageTracker.IncrementCounter(x => x.NumberOfForkPullRequestPulls).Forget();
                    else
                        usageTracker.IncrementCounter(x => x.NumberOfLocalPullRequestPulls).Forget();
                });
        }

        IObservable<Unit> DoPush()
        {
            OperationError = null;

            return pullRequestsService.Push(LocalRepository)
                .Do(_ =>
                {
                    if (IsFromFork)
                        usageTracker.IncrementCounter(x => x.NumberOfForkPullRequestPushes).Forget();
                    else
                        usageTracker.IncrementCounter(x => x.NumberOfLocalPullRequestPushes).Forget();
                });
        }

        async Task DoSyncSubmodules()
        {
            try
            {
                IsBusy = true;
                OperationError = null;
                usageTracker.IncrementCounter(x => x.NumberOfSyncSubmodules).Forget();

                var result = await syncSubmodulesCommand.SyncSubmodules();
                var complete = result.Item1;
                var summary = result.Item2;
                if (!complete)
                {
                    throw new ApplicationException(summary);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        void DoShowReview(IPullRequestReviewSummaryViewModel review)
        {
            if (review.State == PullRequestReviewState.Pending)
            {
                NavigateTo(Invariant($"{RemoteRepositoryOwner}/{LocalRepository.Name}/pull/{Number}/review/new"));
            }
            else
            {
                NavigateTo(Invariant($"{RemoteRepositoryOwner}/{LocalRepository.Name}/pull/{Number}/reviews/{review.User.Login}"));
            }
        }

        void DoShowAnnotations(IPullRequestCheckViewModel checkView)
        {
            NavigateTo(Invariant($"{RemoteRepositoryOwner}/{LocalRepository.Name}/pull/{Number}/checkruns/{checkView.CheckRunId}"));
        }

        class CheckoutCommandState : IPullRequestCheckoutState
        {
            public CheckoutCommandState(string caption, string disabledMessage)
            {
                Caption = caption;
                IsEnabled = disabledMessage == null;
                ToolTip = disabledMessage ?? caption;
            }

            public string Caption { get; }
            public bool IsEnabled { get; }
            public string ToolTip { get; }
        }

        class UpdateCommandState : IPullRequestUpdateState
        {
            public UpdateCommandState(
                BranchTrackingDetails divergence,
                bool pullEnabled,
                bool pushEnabled,
                string pullToolTip,
                string pushToolTip,
                string syncSubmodulesToolTip,
                int submodulesToSync)
            {
                CommitsAhead = divergence.AheadBy ?? 0;
                CommitsBehind = divergence.BehindBy ?? 0;
                PushEnabled = pushEnabled;
                PullEnabled = pullEnabled;
                PullToolTip = pullToolTip;
                PushToolTip = pushToolTip;
                SyncSubmodulesToolTip = syncSubmodulesToolTip;
                SubmodulesToSync = submodulesToSync;
            }

            public int CommitsAhead { get; }
            public int CommitsBehind { get; }
            public bool UpToDate => CommitsAhead == 0 && CommitsBehind == 0 && !SyncSubmodulesEnabled;
            public bool PullEnabled { get; }
            public bool PushEnabled { get; }
            public bool SyncSubmodulesEnabled => SubmodulesToSync > 0;
            public string PullToolTip { get; }
            public string PushToolTip { get; }
            public string SyncSubmodulesToolTip { get; }
            public int SubmodulesToSync { get; }
        }
    }
}
