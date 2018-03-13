using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class PullRequestReviewViewModelDesigner : PanePageViewModelBase, IPullRequestReviewViewModel
    {
        public PullRequestReviewViewModelDesigner()
        {
            PullRequestNumber = 419;
            Model = new PullRequestReviewModel
            {
                SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(1),
                User = new AccountDesigner { Login = "Haacked", IsUser = true },
            };

            Title = "Fix a ton of potential crashers, odd code and redundant calls in ModelService";
            State = PullRequestReviewState.Approved;
            StateDisplay = "approved";
            Body = @"Just a few comments. I don't feel too strongly about them though.

Otherwise, very nice work here! ✨";
            Files = new PullRequestFilesViewModelDesigner();

            var comments = new[]
            {
                new PullRequestReviewCommentModel
                {
                    Body = @"These should probably be properties. Most likely they should be readonly properties. I know that makes creating instances of these not look as nice as using property initializers when constructing an instance, but if these properties should never be mutated after construction, then it guides future consumers to the right behavior.

However, if you're two-way binding these properties to a UI, then ignore the readonly part and make them properties. But in that case they should probably be reactive properties (or implement INPC).",
                    Path = "src/GitHub.Exports.Reactive/ViewModels/IPullRequestListViewModel.cs",
                    Position = 1,
                },
                new PullRequestReviewCommentModel
                {
                    Body = "While I have no problems with naming a variable ass I think we should probably avoid swear words in case Microsoft runs their Policheck tool against this code.",
                    Path = "src/GitHub.App/ViewModels/PullRequestListViewModel.cs",
                    Position = 1,
                },
            };

            FileComments = comments.Select((x, i) => new PullRequestReviewFileCommentViewModel(x, (i * 10) - 1)).ToList();
        }

        public ILocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; }
        public long PullRequestReviewId { get; set; }
        public IPullRequestReviewModel Model { get; set; }
        public string Title { get; set; }
        public PullRequestReviewState State { get; set; }
        public string StateDisplay { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsLatest { get; set; }
        public bool IsPending { get; set; }
        public string Body { get; set; }
        public IPullRequestFilesViewModel Files { get; set; }
        public IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments { get; set; }
        public ReactiveCommand<Unit> OpenComment { get; }
        public ReactiveCommand<object> NavigateToPullRequest { get; }
        public ReactiveCommand<Unit> Submit { get; }

        public Task InitializeAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber,
            long pullRequestReviewId)
        {
            return Task.CompletedTask;
        }

        public Task InitializeNewAsync(ILocalRepositoryModel localRepository, IConnection connection, string owner, string repo, int pullRequestNumber)
        {
            return Task.CompletedTask;
        }

        public Task Load(IPullRequestModel pullRequest)
        {
            return Task.CompletedTask;
        }
    }
}
