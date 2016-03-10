using GitHub.Collections;
using GitHub.Models;
using System.Windows.Input;

namespace GitHub.ViewModels
{
    public interface IPullRequestListViewModel : IViewModel
    {
        ITrackingCollection<IPullRequestModel> PullRequests { get; }
        IPullRequestModel SelectedPullRequest { get; }
        ICommand OpenPullRequest { get; }
    }
}
