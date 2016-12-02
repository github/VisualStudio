using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class PullRequestCheckoutStateDesigner : IPullRequestCheckoutState
    {
        public string Caption { get; set; }
        public string DisabledMessage { get; set; }
    }

    public class PullRequestUpdateStateDesigner : IPullRequestUpdateState
    {
        public int CommitsAhead { get; set; }
        public int CommitsBehind { get; set; }
        public string PullDisabledMessage { get; set; }
        public string PushDisabledMessage { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PullRequestDetailViewModelDesigner : BaseViewModel, IPullRequestDetailViewModel
    {
        public PullRequestDetailViewModelDesigner()
        {
            var repoPath = @"C:\Repo";

            Model = new PullRequestModel(419, 
                "Error handling/bubbling from viewmodels to views to viewhosts",
                 new AccountDesigner { Login = "shana", IsUser = true },
                 DateTime.Now.Subtract(TimeSpan.FromDays(3)))
            {
                State = PullRequestStateEnum.Open,
                CommitCount = 9,
            };

            SourceBranchDisplayName = "shana/error-handling";
            TargetBranchDisplayName = "master";
            Body = @"Adds a way to surface errors from the view model to the view so that view hosts can get to them.

ViewModels are responsible for handling the UI on the view they control, but they shouldn't be handling UI for things outside of the view. In this case, we're showing errors in VS outside the view, and that should be handled by the section that is hosting the view.

This requires that errors be propagated from the viewmodel to the view and from there to the host via the IView interface, since hosts don't usually know what they're hosting.

![An image](https://cloud.githubusercontent.com/assets/1174461/18882991/5dd35648-8496-11e6-8735-82c3a182e8b4.png)";

            var gitHubDir = new PullRequestDirectoryNode("GitHub");
            var modelsDir = new PullRequestDirectoryNode("Models");
            var repositoriesDir = new PullRequestDirectoryNode("Repositories");
            var itrackingBranch = new PullRequestFileNode(repoPath, @"GitHub\Models\ITrackingBranch.cs", PullRequestFileStatus.Modified);
            var oldBranchModel = new PullRequestFileNode(repoPath, @"GitHub\Models\OldBranchModel.cs", PullRequestFileStatus.Removed);
            var concurrentRepositoryConnection = new PullRequestFileNode(repoPath, @"GitHub\Repositories\ConcurrentRepositoryConnection.cs", PullRequestFileStatus.Added);

            repositoriesDir.Files.Add(concurrentRepositoryConnection);
            modelsDir.Directories.Add(repositoriesDir);
            modelsDir.Files.Add(itrackingBranch);
            modelsDir.Files.Add(oldBranchModel);
            gitHubDir.Directories.Add(modelsDir);

            ChangedFilesTree = new ReactiveList<IPullRequestChangeNode>();
            ChangedFilesTree.Add(gitHubDir);

            ChangedFilesList = new ReactiveList<IPullRequestFileNode>();
            ChangedFilesList.Add(concurrentRepositoryConnection);
            ChangedFilesList.Add(itrackingBranch);
            ChangedFilesList.Add(oldBranchModel);
        }

        public IPullRequestModel Model { get; }
        public string SourceBranchDisplayName { get; }
        public string TargetBranchDisplayName { get; }
        public string Body { get; }
        public ChangedFilesViewType ChangedFilesViewType { get; set; }
        public OpenChangedFileAction OpenChangedFileAction { get; set; }
        public IReactiveList<IPullRequestChangeNode> ChangedFilesTree { get; }
        public IReactiveList<IPullRequestFileNode> ChangedFilesList { get; }
        public IPullRequestCheckoutState CheckoutState { get; set; }
        public IPullRequestUpdateState UpdateState { get; set; }
        public string OperationError { get; set; }

        public ReactiveCommand<Unit> Checkout { get; }
        public ReactiveCommand<Unit> Pull { get; }
        public ReactiveCommand<Unit> Push { get; }
        public ReactiveCommand<object> OpenOnGitHub { get; }
        public ReactiveCommand<object> ToggleChangedFilesView { get; }
        public ReactiveCommand<object> ToggleOpenChangedFileAction { get; }
        public ReactiveCommand<object> OpenFile { get; }
        public ReactiveCommand<object> DiffFile { get; }

        public Task<string> ExtractFile(IPullRequestFileNode file)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<string, string>> ExtractDiffFiles(IPullRequestFileNode file)
        {
            throw new NotImplementedException();
        }
    }
}