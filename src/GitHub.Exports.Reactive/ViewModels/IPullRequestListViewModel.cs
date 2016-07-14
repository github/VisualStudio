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
        public bool? IsOpen;
        public string Name;
        public override string ToString()
        {
            return Name;
        }
    }

    public interface IPullRequestListViewModel : IViewModel
    {
        ITrackingCollection<IPullRequestModel> PullRequests { get; }
        IPullRequestModel SelectedPullRequest { get; }
        ICommand OpenPullRequest { get; }
        IReadOnlyList<PullRequestState> States { get; set; }
        PullRequestState SelectedState { get; set; }
        ObservableCollection<IAccount> Authors { get; }
        IAccount SelectedAuthor { get; set; }
        ObservableCollection<IAccount> Assignees { get; }
        IAccount SelectedAssignee { get; set; }
    }
}
