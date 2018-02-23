using System;
using System.Collections.ObjectModel;
using GitHub.Collections;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IIssueListItemViewModel : IViewModel, ICopyable<IIssueListItemViewModel>
    {
        ObservableCollection<IActorViewModel> Assignees { get; }
        IActorViewModel Author { get; }
        string NodeId { get; }
        int Number { get; }
        IssueState State { get; }
        string Title { get; set; }
        DateTimeOffset UpdatedAt { get; }
    }
}