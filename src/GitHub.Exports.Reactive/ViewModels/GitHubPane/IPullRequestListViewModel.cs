using System.Collections.Generic;
using GitHub.Collections;
using GitHub.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GitHub.ViewModels.GitHubPane
{
    public class PullRequestState
    {
        public PullRequestState()
        {
        }

        public PullRequestState(bool isOpen, string name)
        {
            IsOpen = isOpen;
            Name = name;
        }

        public bool? IsOpen;
        public string Name;
        public override string ToString()
        {
            return Name;
        }
    }

    public interface IPullRequestListViewModel : ISearchablePageViewModel, IOpenInBrowser
    {
        IReadOnlyList<IRemoteRepositoryModel> Repositories { get; }
        IRemoteRepositoryModel SelectedRepository { get; set; }
        ITrackingCollection<IPullRequestModel> PullRequests { get; }
        IPullRequestModel SelectedPullRequest { get; }
        IPullRequestModel CheckedOutPullRequest { get; }
        IReadOnlyList<PullRequestState> States { get; set; }
        PullRequestState SelectedState { get; set; }
        ObservableCollection<IAccount> Authors { get; }
        IAccount SelectedAuthor { get; set; }
        ObservableCollection<IAccount> Assignees { get; }
        IAccount SelectedAssignee { get; set; }
        ReactiveCommand<object> OpenPullRequest { get; }
        ReactiveCommand<object> CreatePullRequest { get; }
        ReactiveCommand<object> OpenPullRequestOnGitHub { get; }

        Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);
    }
}
