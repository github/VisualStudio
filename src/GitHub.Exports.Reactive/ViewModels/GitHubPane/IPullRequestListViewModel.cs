using System;
using System.Reactive;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestListViewModel : IIssueListViewModelBase
    {
        ReactiveCommand<object> CreatePullRequest { get; }
    }
}
