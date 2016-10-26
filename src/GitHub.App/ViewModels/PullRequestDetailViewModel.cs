using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
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
    public class PullRequestDetailViewModel : BaseViewModel, IPullRequestDetailViewModel
    {
        readonly ILocalRepositoryModel repository;
        readonly IModelService modelService;
        readonly IPullRequestService pullRequestsService;
        IPullRequestModel model;
        string sourceBranchDisplayName;
        string targetBranchDisplayName;
        string body;
        ChangedFilesViewType changedFilesViewType;
        OpenChangedFileAction openChangedFileAction;
        CheckoutMode checkoutMode;
        string checkoutError;
        int commitsBehind;
        string checkoutDisabledMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDetailViewModel"/> class.
        /// </summary>
        /// <param name="connectionRepositoryHostMap">The connection repository host map.</param>
        /// <param name="teservice">The team explorer service.</param>
        /// <param name="pullRequestsService">The pull requests service.</param>
        /// <param name="avatarProvider">The avatar provider.</param>
        [ImportingConstructor]
        PullRequestDetailViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            ITeamExplorerServiceHolder teservice,
            IPullRequestService pullRequestsService)
            : this(teservice.ActiveRepo,
                  connectionRepositoryHostMap.CurrentRepositoryHost.ModelService,
                  pullRequestsService)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDetailViewModel"/> class.
        /// </summary>
        /// <param name="repositoryHost">The repository host.</param>
        /// <param name="teservice">The team explorer service.</param>
        /// <param name="pullRequestsService">The pull requests service.</param>
        /// <param name="avatarProvider">The avatar provider.</param>
        public PullRequestDetailViewModel(
            ILocalRepositoryModel repository,
            IModelService modelService,
            IPullRequestService pullRequestsService)
        {
            this.repository = repository;
            this.modelService = modelService;
            this.pullRequestsService = pullRequestsService;

            var canCheckout = this.WhenAnyValue(
                x => x.CheckoutMode,
                x => x.CheckoutDisabledMessage,
                (mode, disabled) => mode != CheckoutMode.UpToDate && mode != CheckoutMode.Push && disabled == null);
            Checkout = ReactiveCommand.CreateAsyncObservable(canCheckout, DoCheckout);

            OpenOnGitHub = ReactiveCommand.Create();

            ToggleChangedFilesView = ReactiveCommand.Create();
            ToggleChangedFilesView.Subscribe(_ =>
            {
                ChangedFilesViewType = ChangedFilesViewType == ChangedFilesViewType.TreeView ?
                    ChangedFilesViewType.ListView : ChangedFilesViewType.TreeView;
            });

            ToggleOpenChangedFileAction = ReactiveCommand.Create();
            ToggleOpenChangedFileAction.Subscribe(_ =>
            {
                OpenChangedFileAction = OpenChangedFileAction == OpenChangedFileAction.Diff ?
                    OpenChangedFileAction.Open : OpenChangedFileAction.Diff;
            });

            OpenFile = ReactiveCommand.Create();
            DiffFile = ReactiveCommand.Create();
        }

        /// <summary>
        /// Gets the underlying pull request model.
        /// </summary>
        public IPullRequestModel Model
        {
            get { return model; }
            private set { this.RaiseAndSetIfChanged(ref model, value); }
        }

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
        /// Gets the pull request body.
        /// </summary>
        public string Body
        {
            get { return body; }
            private set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <summary>
        /// Gets or sets a value describing how changed files are displayed in a view.
        /// </summary>
        public ChangedFilesViewType ChangedFilesViewType
        {
            get { return changedFilesViewType; }
            set { this.RaiseAndSetIfChanged(ref changedFilesViewType, value); }
        }

        /// <summary>
        /// Gets or sets a value describing how files are opened when double clicked.
        /// </summary>
        public OpenChangedFileAction OpenChangedFileAction
        {
            get { return openChangedFileAction; }
            set { this.RaiseAndSetIfChanged(ref openChangedFileAction, value); }
        }

        /// <summary>
        /// Gets the checkout mode for the pull request.
        /// </summary>
        public CheckoutMode CheckoutMode
        {
            get { return checkoutMode; }
            private set { this.RaiseAndSetIfChanged(ref checkoutMode, value); }
        }

        /// <summary>
        /// Gets the error message to be displayed below the checkout button.
        /// </summary>
        public string CheckoutError
        {
            get { return checkoutError; }
            private set { this.RaiseAndSetIfChanged(ref checkoutError, value); }
        }

        /// <summary>
        /// Gets the number of commits that the current branch is behind the PR when <see cref="CheckoutMode"/>
        /// is <see cref="CheckoutMode.NeedsPull"/>.
        /// </summary>
        public int CommitsBehind
        {
            get { return commitsBehind; }
            private set { this.RaiseAndSetIfChanged(ref commitsBehind, value); }
        }

        /// <summary>
        /// Gets a message indicating the why the <see cref="Checkout"/> command is disabled.
        /// </summary>
        public string CheckoutDisabledMessage
        {
            get { return checkoutDisabledMessage; }
            private set { this.RaiseAndSetIfChanged(ref checkoutDisabledMessage, value); }
        }

        /// <summary>
        /// Gets the changed files as a tree.
        /// </summary>
        public IReactiveList<IPullRequestChangeNode> ChangedFilesTree { get; } = new ReactiveList<IPullRequestChangeNode>();

        /// <summary>
        /// Gets the changed files as a flat list.
        /// </summary>
        public IReactiveList<IPullRequestFileNode> ChangedFilesList { get; } = new ReactiveList<IPullRequestFileNode>();

        /// <summary>
        /// Gets a command that checks out the pull request locally.
        /// </summary>
        public ReactiveCommand<Unit> Checkout { get; }

        /// <summary>
        /// Gets a command that opens the pull request on GitHub.
        /// </summary>
        public ReactiveCommand<object> OpenOnGitHub { get; }

        /// <summary>
        /// Gets a command that toggles the <see cref="ChangedFilesViewType"/> property.
        /// </summary>
        public ReactiveCommand<object> ToggleChangedFilesView { get; }

        /// <summary>
        /// Gets a command that toggles the <see cref="OpenChangedFileAction"/> property.
        /// </summary>
        public ReactiveCommand<object> ToggleOpenChangedFileAction { get; }

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
            var prNumber = (int)data.Data;

            IsBusy = true;

            modelService.GetPullRequest(repository, prNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Load(x).Forget());
        }

        /// <summary>
        /// Loads the view model from octokit models.
        /// </summary>
        /// <param name="pullRequest">The pull request model.</param>
        /// <param name="files">The pull request's changed files.</param>
        public async Task Load(IPullRequestModel pullRequest)
        {
            Model = pullRequest;
            SourceBranchDisplayName = GetBranchDisplayName(pullRequest.Head?.Label);
            TargetBranchDisplayName = GetBranchDisplayName(pullRequest.Base.Label);
            Body = !string.IsNullOrWhiteSpace(pullRequest.Body) ? pullRequest.Body : "*No description provided.*";

            ChangedFilesTree.Clear();
            ChangedFilesList.Clear();

            // WPF doesn't support AddRange here so iterate through the changes.
            foreach (var change in CreateChangedFilesList(pullRequest.ChangedFiles))
            {
                ChangedFilesList.Add(change);
            }

            foreach (var change in CreateChangedFilesTree(ChangedFilesList).Children)
            {
                ChangedFilesTree.Add(change);
            }

            var localBranches = await pullRequestsService.GetLocalBranches(repository, pullRequest).ToList();
            
            if (localBranches.Contains(repository.CurrentBranch))
            {
                var divergence = await pullRequestsService.CalculateHistoryDivergence(repository, Model.Number);

                if (divergence.BehindBy == null)
                {
                    CheckoutMode = CheckoutMode.InvalidState;
                }
                else if (divergence.AheadBy > 0)
                {
                    CheckoutMode = pullRequestsService.IsPullRequestFromFork(repository, pullRequest) ?
                        CheckoutMode.InvalidState : CheckoutMode.Push;
                }
                else if (divergence.BehindBy == 0)
                {
                    CheckoutMode = CheckoutMode.UpToDate;
                }
                else
                {
                    CheckoutMode = CheckoutMode.NeedsPull;
                    CommitsBehind = divergence.BehindBy.Value;
                }
            }
            else if (localBranches.Count > 0)
            {
                CheckoutMode = CheckoutMode.Switch;
            }
            else
            {
                CheckoutMode = CheckoutMode.Fetch;
            }

            var clean = await pullRequestsService.IsCleanForCheckout(repository);

            CheckoutDisabledMessage = (!clean && CheckoutMode != CheckoutMode.UpToDate && CheckoutMode != CheckoutMode.Push) ?
                $"Cannot {GetCheckoutModeDescription(CheckoutMode)} as your working directory has uncommitted changes." :
                null;

            IsBusy = false;
        }

        /// <summary>
        /// Gets the specified file as it appears in the pull request.
        /// </summary>
        /// <param name="file">The file or directory node.</param>
        /// <returns>The path to the extracted file.</returns>
        public Task<string> ExtractFile(IPullRequestFileNode file)
        {
            var path = Path.Combine(file.Path, file.FileName);
            return pullRequestsService.ExtractFile(repository, model.Head.Sha, path).ToTask();
        }

        /// <summary>
        /// Gets the before and after files needed for viewing a diff.
        /// </summary>
        /// <param name="file">The changed file.</param>
        /// <returns>A tuple containing the full path to the before and after files.</returns>
        public Task<Tuple<string, string>> ExtractDiffFiles(IPullRequestFileNode file)
        {
            var path = Path.Combine(file.Path, file.FileName);
            return pullRequestsService.ExtractDiffFiles(repository, model, path).ToTask();
        }

        static IEnumerable<IPullRequestFileNode> CreateChangedFilesList(IEnumerable<IPullRequestFileModel> files)
        {
            return files.Select(x => new PullRequestFileNode(x.FileName, x.Status));
        }

        static IPullRequestDirectoryNode CreateChangedFilesTree(IEnumerable<IPullRequestFileNode> files)
        {
            var dirs = new Dictionary<string, PullRequestDirectoryNode>
            {
                { string.Empty, new PullRequestDirectoryNode(string.Empty) }
            };

            foreach (var file in files)
            {
                var dir = GetDirectory(file.Path, dirs);
                dir.Files.Add(file);
            }

            return dirs[string.Empty];
        }

        static string GetCheckoutModeDescription(CheckoutMode checkoutMode)
        {
            switch (checkoutMode)
            {
                case CheckoutMode.NeedsPull:
                    return "update branch";
                case CheckoutMode.Switch:
                    return "switch branches";
                case CheckoutMode.Fetch:
                case CheckoutMode.InvalidState:
                    return "checkout pull request";
                default:
                    Debug.Fail("Invalid CheckoutMode in GetCheckoutModeDescription");
                    return null;
            }
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

        string GetBranchDisplayName(string targetBranchLabel)
        {
            if (targetBranchLabel != null)
            {
                var parts = targetBranchLabel.Split(':');
                var owner = parts[0];
                return owner == repository.CloneUrl.Owner ? parts[1] : targetBranchLabel;
            }
            else
            {
                return "[Invalid]";
            }
        }

        IObservable<Unit> DoCheckout(object unused)
        {
            IObservable<Unit> operation = null;

            switch (CheckoutMode)
            {
                case CheckoutMode.NeedsPull:
                    operation = pullRequestsService.Pull(repository);
                    break;
                case CheckoutMode.Fetch:
                    operation = pullRequestsService
                        .GetDefaultLocalBranchName(repository, Model.Number, Title)
                        .SelectMany(x => pullRequestsService.FetchAndCheckout(repository, Model.Number, x));
                    break;
                case CheckoutMode.Switch:
                    operation = pullRequestsService.SwitchToBranch(repository, Model);
                    break;
                case CheckoutMode.InvalidState:
                    operation = pullRequestsService
                        .UnmarkLocalBranch(repository)
                        .SelectMany(_ => pullRequestsService.GetDefaultLocalBranchName(repository, Model.Number, Title))
                        .SelectMany(x => pullRequestsService.FetchAndCheckout(repository, Model.Number, x));
                    break;
                default:
                    Debug.Fail("Invalid CheckoutMode in PullRequestDetailViewModel.DoCheckout.");
                    operation = Observable.Empty<Unit>();
                    break;
            }

            return operation.Catch<Unit, Exception>(ex =>
            {
                CheckoutError = ex.Message;
                return Observable.Empty<Unit>();
            });
        }
    }
}
