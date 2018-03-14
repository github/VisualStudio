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
            PullRequestTitle = "Error handling/bubbling from viewmodels to views to viewhosts";
            Reviews = new[]
            {
                new PullRequestReviewViewModelDesigner()
                {
                    IsExpanded = true,
                    HasDetails = true,
                    FileComments = new PullRequestReviewFileCommentViewModel[0],
                    StateDisplay = "approved",
                    Model = new PullRequestReviewModel
                    {
                        State = PullRequestReviewState.Approved,
                        SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(1),
                        User = User,
                    },
                },
                new PullRequestReviewViewModelDesigner()
                {
                    IsExpanded = true,
                    HasDetails = true,
                    StateDisplay = "requested changes",
                    Model = new PullRequestReviewModel
                    {
                        State = PullRequestReviewState.ChangesRequested,
                        SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(2),
                        User = User,
                    },
                },
                new PullRequestReviewViewModelDesigner()
                {
                    IsExpanded = false,
                    HasDetails = false,
                    StateDisplay = "commented",
                    Model = new PullRequestReviewModel
                    {
                        State = PullRequestReviewState.Commented,
                        SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(2),
                        User = User,
                    },
                }
            };
        }

        public ILocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; }
        public IAccount User { get; set; }
        public IReadOnlyList<IPullRequestReviewViewModel> Reviews { get; set; }
        public string PullRequestTitle { get; set; }
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
