using System.Collections.Generic;
using GitHub.Collections;
using GitHub.Models;
using System.Windows.Input;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace GitHub.ViewModels
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

    public interface IPullRequestListViewModel : IViewModel, ICanNavigate, IHasBusy
    {
        IReadOnlyList<IRemoteRepositoryModel> Repositories { get; }
        IRemoteRepositoryModel SelectedRepository { get; set; }
        ITrackingCollection<IPullRequestModel> PullRequests { get; }
        IPullRequestModel SelectedPullRequest { get; }
        IReadOnlyList<PullRequestState> States { get; set; }
        PullRequestState SelectedState { get; set; }
        ObservableCollection<IAccount> Authors { get; }
        IAccount SelectedAuthor { get; set; }
        ObservableCollection<IAccount> Assignees { get; }
        IAccount SelectedAssignee { get; set; }
        string FilterText { get; set; }
        ReactiveCommand<object> OpenPullRequest { get; }
        ReactiveCommand<object> CreatePullRequest { get; }
    }
}
