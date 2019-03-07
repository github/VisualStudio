using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestUserReviewsViewModelDesigner : PanePageViewModelBase, IPullRequestUserReviewsViewModel
    {
        public PullRequestUserReviewsViewModelDesigner()
        {
            var userModel = new ActorModel { Login = "Haacked" };

            User = new ActorViewModel(userModel);
            PullRequestNumber = 123;
            PullRequestTitle = "Error handling/bubbling from viewmodels to views to viewhosts";

            Reviews = new[]
            {
                new PullRequestReviewViewModelDesigner()
                {
                    IsExpanded = true,
                    HasDetails = true,
                    FileComments = Array.Empty<IPullRequestReviewFileCommentViewModel>(),
                    StateDisplay = "approved",
                    Model = new PullRequestReviewModel
                    {
                        State = PullRequestReviewState.Approved,
                        SubmittedAt = DateTimeOffset.Now - TimeSpan.FromDays(1),
                        Author = userModel,
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
                        Author = userModel,
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
                        Author = userModel,
                    },
                }
            };
        }

        public LocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; }
        public IActorViewModel User { get; set; }
        public IReadOnlyList<IPullRequestReviewViewModel> Reviews { get; set; }
        public string PullRequestTitle { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }

        public Task InitializeAsync(LocalRepositoryModel localRepository, IConnection connection, string owner, string repo, int pullRequestNumber, string login)
        {
            return Task.CompletedTask;
        }
    }
}
