using System.Collections.Generic;
using GitHub.Collections;
using GitHub.Models;
using System.Windows.Input;
using ReactiveUI;
using System.Collections.ObjectModel;
using GitHub.Settings;

namespace GitHub.ViewModels
{
    public class PullRequestState : NamedItemContainer
    {
        public PullRequestState(string name, bool? isOpen = null) : base(name)
        {
            IsOpen = isOpen;
        }

        public bool? IsOpen { get; }
    }

    public class PullRequestSortOrder : NamedItemContainer
    {
        public PullRequestSortOrder(string name, IComparer<IPullRequestModel> comparer, SortOrder sortOrder) : base(name)
        {
            Comparer = comparer;
            SortOrder = sortOrder;
        }

        public IComparer<IPullRequestModel> Comparer { get; }
        public SortOrder SortOrder { get; }
    }

    public interface IPullRequestListViewModel : IViewModel
    {
        ITrackingCollection<IPullRequestModel> PullRequests { get; }
        IPullRequestModel SelectedPullRequest { get; }
        ICommand OpenPullRequest { get; }
        IReadOnlyList<PullRequestState> States { get; }
        PullRequestState SelectedState { get; set; }
        ObservableCollection<IAccount> Authors { get; }
        IAccount SelectedAuthor { get; set; }
        ObservableCollection<IAccount> Assignees { get; }
        IAccount SelectedAssignee { get; set; }
        IReadOnlyList<PullRequestSortOrder> SortOrders { get; }
        PullRequestSortOrder SelectedSortOrder { get; set; }
    }
}
