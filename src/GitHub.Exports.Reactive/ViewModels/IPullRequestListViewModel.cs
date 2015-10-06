using GitHub.Collections;
using GitHub.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GitHub.ViewModels
{
    public interface IPullRequestListViewModel : IViewModel
    {
        ITrackingCollection<IPullRequestModel> PullRequests { get; }
        IPullRequestModel SelectedPullRequest { get; }
    }
}
