using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using LibGit2Sharp;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A view model which displays the details of a pull request.
    /// </summary>
    [ExportViewModel(ViewType = UIViewType.PRDetail)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [NullGuard(ValidationFlags.None)]
    public class PullRequestDetailViewModel : PanePageViewModelBase, IPullRequestDetailViewModel
    {
        readonly IModelService modelService;
        readonly IPullRequestService pullRequestsService;
        readonly IPullRequestSessionManager sessionManager;
        readonly IUsageTracker usageTracker;
        IPullRequestModel model;
        string sourceBranchDisplayName;
        string targetBranchDisplayName;
        int commentCount;
        string body;
        IReadOnlyList<IPullRequestChangeNode> changedFilesTree;
        IPullRequestCheckoutState checkoutState;
        IPullRequestUpdateState updateState;
        string errorMessage;
        string operationError;
        bool isBusy;
        bool isLoading;
        bool isCheckedOut;
        bool isFromFork;
        bool isInCheckout;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDetailViewModel"/> class.
        /// </summary>
        /// <param name="connectionRepositoryHostMap">The connection repository host map.</param>
        /// <param name="teservice">The team explorer service.</param>
        /// <param name="pullRequestsService">The pull requests service.</param>
        /// <param name="sessionManager">The pull request session manager.</param>
        /// <param name="usageTracker">The usage tracker.</param>
        [ImportingConstructor]
        PullRequestDetailViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            ITeamExplorerServiceHolder teservice,
            IPullRequestService pullRequestsService,
            IPullRequestSessionManager sessionManager,
            IUsageTracker usageTracker)
            : this(teservice.ActiveRepo,
                   connectionRepositoryHostMap.CurrentRepositoryHost.ModelService,
                   pullRequestsService,
                   sessionManager,
                   usageTracker)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDetailViewModel"/> class.
        /// </summary>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="modelService">The model service.</param>
        /// <param name="pullRequestsService">The pull requests service.</param>
        /// <param name="sessionManager">The pull request session manager.</param>
        /// <param name="usageTracker">The usage tracker.</param>
        public PullRequestDetailViewModel(
            ILocalRepositoryModel localRepository,
            IModelService modelService,
            IPullRequestService pullRequestsService,
            IPullRequestSessionManager sessionManager,
            IUsageTracker usageTracker)
        {
            this.LocalRepository = localRepository;
            this.modelService = modelService;
            this.pullRequestsService = pullRequestsService;
            this.sessionManager = sessionManager;
            this.usageTracker = usageTracker;

            Checkout = ReactiveCommand.CreateAsyncObservable(
                this.WhenAnyValue(x => x.CheckoutState)
                    .Cast<CheckoutCommandState>()
                    .Select(x => x != null && x.IsEnabled), 
                DoCheckout);
            Checkout.IsExecuting.Subscribe(x => isInCheckout = x);
            SubscribeOperationError(Checkout);

            Pull = ReactiveCommand.CreateAsyncObservable(
                this.WhenAnyValue(x => x.UpdateState)
                    .Cast<UpdateCommandState>()
                    .Select(x => x != null && x.PullEnabled),
                DoPull);
            SubscribeOperationError(Pull);

            Push = ReactiveCommand.CreateAsyncObservable(
                this.WhenAnyValue(x => x.UpdateState)
                    .Cast<UpdateCommandState>()
                    .Select(x => x != null && x.PushEnabled),
                DoPush);
            SubscribeOperationError(Push);

            OpenOnGitHub = ReactiveCommand.Create();
            OpenFile = ReactiveCommand.Create(this.WhenAnyValue(x => x.IsCheckedOut));
            DiffFile = ReactiveCommand.Create();
        }

        /// <summary>
        /// Gets the underlying pull request model.
        /// </summary>
        public IPullRequestModel Model
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

        /// <summary>
        /// Gets the local repository.
        /// </summary>
        public ILocalRepositoryModel LocalRepository { get; }

        /// <summary>
        /// Gets the remote repository that contains the pull request.
        /// </summary>
        /// <remarks>
        /// The remote repository may be different from the local repository if the local
        /// repository is a fork and the user is viewing pull requests from the parent repository.
        /// </remarks>
        public IRemoteRepositoryModel RemoteRepository { get; private set; }

        /// <summary>
        /// Gets the session for the pull request.
        /// </summary>
        public IPullRequestSession Session { get; private set; }

        /// <summary>
        /// Gets a string describing how to display the pull request's source branch.
        /// </summary>
        public string SourceBranchDisplayName
        {
            get { return sourceBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref sourceBranchDisplayName, value); }
        }

        /// <summary>
        /// Gets a string describing how to display the pull request's target branch.
        /// </summary>
        public string TargetBranchDisplayName
        {
            get { return targetBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref targetBranchDisplayName, value); }
        }

        /// <summary>
        /// Gets the number of comments made on the pull request.
        /// </summary>
        public int CommentCount
        {
            get { return commentCount; }
            private set { this.RaiseAndSetIfChanged(ref commentCount, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the view model is updating.
        /// </summary>
        public bool IsBusy
        {
            get { return isBusy; }
            private set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        /// Gets a value indicating whether the pull request branch is checked out.
        /// </summary>
        public bool IsCheckedOut
        {
            get { return isCheckedOut; }
            private set { this.RaiseAndSetIfChanged(ref isCheckedOut, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the view model is loading.
        /// </summary>
        public bool IsLoading
        {
            get { return isLoading; }
            private set { this.RaiseAndSetIfChanged(ref isLoading, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the pull request comes from a fork.
        /// </summary>
        public bool IsFromFork
        {
            get { return isFromFork; }
            private set { this.RaiseAndSetIfChanged(ref isFromFork, value); }
        }

        /// <summary>
        /// Gets the pull request body.
        /// </summary>
        public string Body
        {
            get { return body; }
            private set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <summary>
        /// Gets the state associated with the <see cref="Checkout"/> command.
        /// </summary>
        public IPullRequestCheckoutState CheckoutState
        {
            get { return checkoutState; }
            private set { this.RaiseAndSetIfChanged(ref checkoutState, value); }
        }

        /// <summary>
        /// Gets the state associated with the <see cref="Pull"/> and <see cref="Push"/> commands.
        /// </summary>
        public IPullRequestUpdateState UpdateState
        {
            get { return updateState; }
            private set { this.RaiseAndSetIfChanged(ref updateState, value); }
        }

        /// <summary>
        /// Gets an error message to display if loading fails.
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
            private set { this.RaiseAndSetIfChanged(ref errorMessage, value); }
        }

        /// <summary>
        /// Gets the error message to be displayed in the action area as a result of an error in a
        /// git operation.
        /// </summary>
        public string OperationError
        {
            get { return operationError; }
            private set { this.RaiseAndSetIfChanged(ref operationError, value); }
        }

        /// <summary>
        /// Gets the changed files as a tree.
        /// </summary>
        public IReadOnlyList<IPullRequestChangeNode> ChangedFilesTree
        {
            get { return changedFilesTree; }
            private set { this.RaiseAndSetIfChanged(ref changedFilesTree, value); }
        }

        /// <summary>
        /// Gets a command that checks out the pull request locally.
        /// </summary>
        public ReactiveCommand<Unit> Checkout { get; }

        /// <summary>
        /// Gets a command that pulls changes to the current branch.
        /// </summary>
        public ReactiveCommand<Unit> Pull { get; }

        /// <summary>
        /// Gets a command that pushes changes from the current branch.
        /// </summary>
        public ReactiveCommand<Unit> Push { get; }

        /// <summary>
        /// Gets a command that opens the pull request on GitHub.
        /// </summary>
        public ReactiveCommand<object> OpenOnGitHub { get; }

        /// <summary>
        /// Gets a command that opens a <see cref="IPullRequestFileNode"/>.
        /// </summary>
        public ReactiveCommand<object> OpenFile { get; }

        /// <summary>
        /// Gets a command that diffs a <see cref="IPullRequestFileNode"/>.
        /// </summary>
        public ReactiveCommand<object> DiffFile { get; }

        /// <summary>
        /// Initializes the view model with new data.
        /// </summary>
        /// <param name="data"></param>
        public override void Initialize([AllowNull] ViewWithData data)
        {
            int number;
            var repo = RemoteRepository;

            if (data != null)
            {
                var arg = (PullRequestDetailArgument)data.Data;
                number = arg.Number;
                repo = arg.Repository;
                IsLoading = true;
            }
            else
            {
                number = Model.Number;
                IsBusy = true;
            }

            ErrorMessage = OperationError = null;
            modelService.GetPullRequest(repo, number)
                .TakeLast(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IPullRequestModel, Exception>(ex =>
                {
                    ErrorMessage = ex.Message.Trim();
                    IsLoading = IsBusy = false;
                    return Observable.Empty<IPullRequestModel>();
                })
                .Subscribe(x => Load(repo, x).Forget());
        }

        /// <summary>
        /// Loads the view model from octokit models.
        /// </summary>
        /// <param name="remoteRepository">The remote repository.</param>
        /// <param name="pullRequest">The pull request model.</param>
        public async Task Load(IRemoteRepositoryModel remoteRepository, IPullRequestModel pullRequest)
        {
            try
            {
                var firstLoad = (Model == null);
                Model = pullRequest;
                Session = await sessionManager.GetSession(pullRequest);
                Title = Resources.PullRequestNavigationItemText + " #" + pullRequest.Number;

                IsFromFork = !pullRequestsService.IsPullRequestFromRepository(LocalRepository, Model);
                SourceBranchDisplayName = GetBranchDisplayName(IsFromFork, pullRequest.Head?.Label);
                TargetBranchDisplayName = GetBranchDisplayName(IsFromFork, pullRequest.Base.Label);
                CommentCount = pullRequest.Comments.Count + pullRequest.ReviewComments.Count;
                Body = !string.IsNullOrWhiteSpace(pullRequest.Body) ? pullRequest.Body : Resources.NoDescriptionProvidedMarkdown;

                var changes = await pullRequestsService.GetTreeChanges(LocalRepository, pullRequest);
                ChangedFilesTree = (await CreateChangedFilesTree(pullRequest, changes)).Children.ToList();

                var localBranches = await pullRequestsService.GetLocalBranches(LocalRepository, pullRequest).ToList();

                IsCheckedOut = localBranches.Contains(LocalRepository.CurrentBranch);

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

                    UpdateState = new UpdateCommandState(divergence, pullEnabled, pushEnabled, pullToolTip, pushToolTip);
                    CheckoutState = null;
                }
                else
                {
                    var caption = localBranches.Count > 0 ?
                        string.Format(Resources.PullRequestDetailsCheckout, localBranches.First().DisplayName) :
                        string.Format(Resources.PullRequestDetailsCheckoutTo, await pullRequestsService.GetDefaultLocalBranchName(LocalRepository, Model.Number, Model.Title));
                    var clean = await pullRequestsService.IsWorkingDirectoryClean(LocalRepository);
                    string disabled = null;

                    if (pullRequest.Head == null || !pullRequest.Head.RepositoryCloneUrl.IsValidUri)
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

                if (firstLoad)
                {
                    usageTracker.IncrementPullRequestOpened().Forget();
                }

                if (!isInCheckout)
                {
                    pullRequestsService.RemoveUnusedRemotes(LocalRepository).Subscribe(_ => { });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message.Trim();
            }
            finally
            {
                IsLoading = IsBusy = false;
            }
        }

        /// <summary>
        /// Gets the before and after files needed for viewing a diff.
        /// </summary>
        /// <param name="file">The changed file.</param>
        /// <returns>A tuple containing the full path to the before and after files.</returns>
        public Task<Tuple<string, string>> ExtractDiffFiles(IPullRequestFileNode file)
        {
            var path = Path.Combine(file.DirectoryPath, file.FileName);
            return pullRequestsService.ExtractDiffFiles(LocalRepository, model, path, IsCheckedOut).ToTask();
        }

        /// <summary>
        /// Gets the full path to a file in the working directory.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The full path to the file in the working directory.</returns>
        public string GetLocalFilePath(IPullRequestFileNode file)
        {
            return Path.Combine(LocalRepository.LocalPath, file.DirectoryPath, file.FileName);
        }

        void SubscribeOperationError(ReactiveCommand<Unit> command)
        {
            command.ThrownExceptions.Subscribe(x => OperationError = x.Message);
            command.IsExecuting.Select(x => x).Subscribe(x => OperationError = null);
        }

        async Task<IPullRequestDirectoryNode> CreateChangedFilesTree(IPullRequestModel pullRequest, TreeChanges changes)
        {
            var dirs = new Dictionary<string, PullRequestDirectoryNode>
            {
                { string.Empty, new PullRequestDirectoryNode(string.Empty) }
            };

            foreach (var changedFile in pullRequest.ChangedFiles)
            {
                var node = new PullRequestFileNode(
                    LocalRepository.LocalPath,
                    changedFile.FileName,
                    changedFile.Sha,
                    changedFile.Status,
                    GetStatusDisplay(changedFile, changes));

                var file = await Session.GetFile(changedFile.FileName);
                var fileCommentCount = file?.WhenAnyValue(x => x.InlineCommentThreads)
                    .Subscribe(x => node.CommentCount = x.Count(y => y.LineNumber != -1));

                var dir = GetDirectory(node.DirectoryPath, dirs);
                dir.Files.Add(node);
            }

            return dirs[string.Empty];
        }

        static PullRequestDirectoryNode GetDirectory(string path, Dictionary<string, PullRequestDirectoryNode> dirs)
        {
            PullRequestDirectoryNode dir;

            if (!dirs.TryGetValue(path, out dir))
            {
                var parentPath = Path.GetDirectoryName(path);
                var parentDir = GetDirectory(parentPath, dirs);

                dir = new PullRequestDirectoryNode(path);

                if (!parentDir.Directories.Any(x => x.DirectoryName == dir.DirectoryName))
                {
                    parentDir.Directories.Add(dir);
                    dirs.Add(path, dir);
                }
            }

            return dir;
        }

        static string GetBranchDisplayName(bool isFromFork, string targetBranchLabel)
        {
            if (targetBranchLabel != null)
            {
                return isFromFork ? targetBranchLabel : targetBranchLabel.Split(':')[1];
            }
            else
            {
                return Resources.InvalidBranchName;
            }
        }

        string GetStatusDisplay(IPullRequestFileModel file, TreeChanges changes)
        {
            switch (file.Status)
            {
                case PullRequestFileStatus.Added:
                    return Resources.AddedFileStatus;
                case PullRequestFileStatus.Renamed:
                    var fileName = file.FileName.Replace("/", "\\");
                    var change = changes?.Renamed.FirstOrDefault(x => x.Path == fileName);

                    if (change != null)
                    {
                        return Path.GetDirectoryName(change.OldPath) == Path.GetDirectoryName(change.Path) ?
                            Path.GetFileName(change.OldPath) : change.OldPath;
                    }
                    else
                    {
                        return Resources.RenamedFileStatus;
                    }
                default:
                    return null;
            }
        }

        IObservable<Unit> DoCheckout(object unused)
        {
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
            }).Do(_ => usageTracker.IncrementPullRequestCheckOutCount(IsFromFork).Forget());
        }

        IObservable<Unit> DoPull(object unused)
        {
            return pullRequestsService.Pull(LocalRepository)
                .Do(_ => usageTracker.IncrementPullRequestPullCount(IsFromFork).Forget());
        }

        IObservable<Unit> DoPush(object unused)
        {
            return pullRequestsService.Push(LocalRepository)
                .Do(_ => usageTracker.IncrementPullRequestPushCount(IsFromFork).Forget());
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
                string pushToolTip)
            {
                CommitsAhead = divergence.AheadBy ?? 0;
                CommitsBehind = divergence.BehindBy ?? 0;
                PushEnabled = pushEnabled;
                PullEnabled = pullEnabled;
                PullToolTip = pullToolTip;
                PushToolTip = pushToolTip;
            }

            public int CommitsAhead { get; }
            public int CommitsBehind { get; }
            public bool UpToDate => CommitsAhead == 0 && CommitsBehind == 0;
            public bool PullEnabled { get; }
            public bool PushEnabled { get; }
            public string PullToolTip { get; }
            public string PushToolTip { get; }
        }
    }
}
