using GitHub.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace GitHub.ViewModels
{
    public interface IPullRequestListViewModel
    {
        IReactiveDerivedList<IPullRequestModel> PullRequests { get; }
        IPullRequestModel SelectedPullRequest { get; }
    }
}
