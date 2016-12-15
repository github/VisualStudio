﻿using System.Collections.Generic;
using GitHub.Collections;
using GitHub.Models;
using System.Windows.Input;
using ReactiveUI;
using System.Collections.ObjectModel;
using GitHub.Settings;

namespace GitHub.ViewModels
{
    public class PullRequestState : ViewModelItemContainer
    {
        public bool? IsOpen;
    }

    public class PullRequestSortOrder : ViewModelItemContainer
    {
        public IComparer<IPullRequestModel> Comparer;
        public SortOrder SortOrder;
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
