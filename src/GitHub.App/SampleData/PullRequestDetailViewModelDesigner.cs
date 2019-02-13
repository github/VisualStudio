using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.SampleData;
using ReactiveUI.Legacy;

namespace GitHub.SampleData
{
    public class PullRequestCheckoutStateDesigner : IPullRequestCheckoutState
    {
        public string Caption { get; set; }
        public bool IsEnabled { get; set; }
        public string ToolTip { get; set; }
    }

    public class PullRequestUpdateStateDesigner : IPullRequestUpdateState
    {
        public int CommitsAhead { get; set; }
        public int CommitsBehind { get; set; }
        public bool UpToDate { get; set; }
        public string PullToolTip { get; set; }
        public string PushToolTip { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PullRequestDetailViewModelDesigner : PanePageViewModelBase, IPullRequestDetailViewModel
    {
        public PullRequestDetailViewModelDesigner()
        {
            var repoPath = @"C:\Repo";

            Model = new PullRequestDetailModel
            {
                Number = 419,
                Title = "Error handling/bubbling from viewmodels to views to viewhosts",
                Author = new ActorModel { Login = "shana" },
                UpdatedAt = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(3)),
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
            var itrackingBranch = new PullRequestFileNode(repoPath, @"GitHub\Models\ITrackingBranch.cs", "abc", PullRequestFileStatus.Modified, null);
            var oldBranchModel = new PullRequestFileNode(repoPath, @"GitHub\Models\OldBranchModel.cs", "abc", PullRequestFileStatus.Removed, null);
            var concurrentRepositoryConnection = new PullRequestFileNode(repoPath, @"GitHub\Repositories\ConcurrentRepositoryConnection.cs", "abc", PullRequestFileStatus.Added, null);

            repositoriesDir.Files.Add(concurrentRepositoryConnection);
            modelsDir.Directories.Add(repositoriesDir);
            modelsDir.Files.Add(itrackingBranch);
            modelsDir.Files.Add(oldBranchModel);
            gitHubDir.Directories.Add(modelsDir);

            Reviews = new[]
            {
                new PullRequestReviewSummaryViewModel
                {
                    Id = "id1",
                    User = new ActorViewModel { Login = "grokys" },
                    State = PullRequestReviewState.Pending,
                    FileCommentCount = 0,
                },
                new PullRequestReviewSummaryViewModel
                {
                    Id = "id",
                    User = new ActorViewModel { Login = "jcansdale" },
                    State = PullRequestReviewState.Approved,
                    FileCommentCount = 5,
                },
                new PullRequestReviewSummaryViewModel
                {
                    Id = "id3",
                    User = new ActorViewModel { Login = "shana" },
                    State = PullRequestReviewState.ChangesRequested,
                    FileCommentCount = 5,
                },
                new PullRequestReviewSummaryViewModel
                {
                },
            };

            Files = new PullRequestFilesViewModelDesigner();

            Checks = Array.Empty<PullRequestCheckViewModelDesigner>();
        }

        public PullRequestDetailModel Model { get; }
        public IPullRequestSession Session { get; }
        public LocalRepositoryModel LocalRepository { get; }
        public string RemoteRepositoryOwner { get; }
        public int Number { get; set; }
        public IActorViewModel Author { get; set; }
        public string SourceBranchDisplayName { get; set; }
        public string TargetBranchDisplayName { get; set; }
        public int CommentCount { get; set; }
        public bool IsCheckedOut { get; }
        public bool IsFromFork { get; }
        public string Body { get; }
        public IReadOnlyList<IPullRequestReviewSummaryViewModel> Reviews { get; }
        public IPullRequestFilesViewModel Files { get; set; }
        public IPullRequestCheckoutState CheckoutState { get; set; }
        public IPullRequestUpdateState UpdateState { get; set; }
        public string OperationError { get; set; }
        public string ErrorMessage { get; set; }
        public Uri WebUrl { get; set; }

        public ReactiveCommand<Unit, Unit> Checkout { get; }
        public ReactiveCommand<Unit, Unit> Pull { get; }
        public ReactiveCommand<Unit, Unit> Push { get; }
        public ReactiveCommand<Unit, Unit> SyncSubmodules { get; }
        public ReactiveCommand<Unit, Unit> OpenOnGitHub { get; }
        public ReactiveCommand<IPullRequestReviewSummaryViewModel, Unit> ShowReview { get; }
        public ReactiveCommand<IPullRequestCheckViewModel, Unit> ShowAnnotations { get; }

        public IReadOnlyList<IPullRequestCheckViewModel> Checks { get; }

        public Task InitializeAsync(LocalRepositoryModel localRepository, IConnection connection, string owner, string repo, int number) => Task.CompletedTask;

        public string GetLocalFilePath(IPullRequestFileNode file)
        {
            return null;
        }
    }
}