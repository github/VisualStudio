using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestUserReviewsViewModel : IPanePageViewModel
    {
        ILocalRepositoryModel LocalRepository { get; }
        ReactiveCommand<object> NavigateToPullRequest { get; }
        int PullRequestNumber { get; }
        string RemoteRepositoryOwner { get; }
        IReadOnlyList<IPullRequestReviewViewModel> Reviews { get; }
        string Title { get; }
        IAccount User { get; }

        Task InitializeAsync(ILocalRepositoryModel localRepository, IConnection connection, string owner, string repo, int pullRequestNumber, string login);
        Task Load(IAccount user, IPullRequestModel pullRequest);
    }
}