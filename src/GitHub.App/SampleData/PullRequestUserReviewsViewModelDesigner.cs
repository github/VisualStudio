using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class PullRequestUserReviewsViewModelDesigner : PanePageViewModelBase, IPullRequestUserReviewsViewModel
    {
        public PullRequestUserReviewsViewModelDesigner()
        {
            User = new AccountDesigner { Login = "Haacked", IsUser = true };
            PullRequestNumber = 123;
            Title = "Error handling/bubbling from viewmodels to views to viewhosts";
            Reviews = new[]
            {
                new PullRequestReviewViewModelDesigner()
                {
                    Body = null,
                    IsEmpty = true,
                    IsLatest = true,
                    FileComments = new PullRequestReviewFileCommentViewModel[0],
                    State = PullRequestReviewState.Approved,
                    StateDisplay = "approved",
                },
                new PullRequestReviewViewModelDesigner()
                {
                    IsLatest = true,
                    State = PullRequestReviewState.ChangesRequested,
                    StateDisplay = "requested changes",
                }
            };
        }

        public ILocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; }
        public IAccount User { get; set; }
        public IReadOnlyList<IPullRequestReviewViewModel> Reviews { get; set; }
        public string Title { get; set; }
        public ReactiveCommand<object> NavigateToPullRequest { get; }

        public Task InitializeAsync(ILocalRepositoryModel localRepository, IConnection connection, string owner, string repo, int pullRequestNumber, string login)
        {
            return Task.CompletedTask;
        }

        public Task Load(IAccount user, IPullRequestModel pullRequest)
        {
            return Task.CompletedTask;
        }
    }
}
