using System;
using System.Collections.Generic;
using GitHub.Collections;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IIssueListItemViewModel : IViewModel, ICopyable<IIssueListItemViewModel>
    {
        IReadOnlyList<IActorViewModel> Assignees { get; }
        IActorViewModel Author { get; }
        int CommentCount { get; }
        IReadOnlyList<IssueLabelModel> Labels { get; }
        string NodeId { get; }
        int Number { get; }
        IssueState State { get; }
        string Title { get; set; }
        DateTimeOffset UpdatedAt { get; }
    }
}