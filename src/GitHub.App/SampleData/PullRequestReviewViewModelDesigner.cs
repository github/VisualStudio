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
            PullRequestNumber = 734;

            Model = new PullRequestReviewModel
            {
                User = new AccountDesigner { Login = "Haacked", IsUser = true },
            };

            State = "approved";
            Body = @"Just a few comments. I don't feel too strongly about them though.

Otherwise, very nice work here! ✨";
            Files = new PullRequestFilesViewModelDesigner();
        }

        public ILocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; }
        public long PullRequestReviewId { get; set; }
        public IPullRequestReviewModel Model { get; set; }
        public string State { get; set; }
        public string Body { get; set; }
        public IPullRequestFilesViewModel Files { get; set; }
        public ReactiveCommand<object> NavigateToPullRequest { get; }

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

        public Task Load(IPullRequestModel pullRequest)
        {
            return Task.CompletedTask;
        }
    }
}
