using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class PullRequestReviewAuthoringViewModelDesigner : PanePageViewModelBase, IPullRequestReviewAuthoringViewModel
    {
        public PullRequestReviewAuthoringViewModelDesigner()
        {
            PullRequestModel = new PullRequestDetailModel
            {
                Number = 419,
                Title = "Fix a ton of potential crashers, odd code and redundant calls in ModelService",
                Author = new ActorModel { Login = "Haacked" },
                UpdatedAt =  DateTimeOffset.Now - TimeSpan.FromDays(2),
            };

            Files = new PullRequestFilesViewModelDesigner();

            FileComments = new[]
            {
                new PullRequestReviewFileCommentViewModelDesigner
                {
                    Body = @"These should probably be properties. Most likely they should be readonly properties. I know that makes creating instances of these not look as nice as using property initializers when constructing an instance, but if these properties should never be mutated after construction, then it guides future consumers to the right behavior.

However, if you're two-way binding these properties to a UI, then ignore the readonly part and make them properties. But in that case they should probably be reactive properties (or implement INPC).",
                    RelativePath = "src/GitHub.Exports.Reactive/ViewModels/IPullRequestListViewModel.cs",
                },
                new PullRequestReviewFileCommentViewModelDesigner
                {
                    Body = "While I have no problems with naming a variable ass I think we should probably avoid swear words in case Microsoft runs their Policheck tool against this code.",
                    RelativePath = "src/GitHub.App/ViewModels/PullRequestListViewModel.cs",
                },
            };
        }

        public string Body { get; set; }
        public bool CanApproveRequestChanges { get; set; }
        public IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments { get; }
        public IPullRequestFilesViewModel Files { get; }
        public LocalRepositoryModel LocalRepository { get; set; }
        public PullRequestReviewModel Model { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }
        public string OperationError { get; set; }
        public PullRequestDetailModel PullRequestModel { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public ReactiveCommand<Unit, Unit> Approve { get; }
        public ReactiveCommand<Unit, Unit> Comment { get; }
        public ReactiveCommand<Unit, Unit> RequestChanges { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public Task InitializeAsync(
            LocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber)
        {
            throw new NotImplementedException();
        }
    }
}
