using System;
using GitHub.Collections;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IIssueListItemViewModel : IViewModel, ICopyable<IIssueListItemViewModel>
    {
        IActorViewModel Author { get; }
        string NodeId { get; }
        int Number { get; }
        string Title { get; set; }
        DateTimeOffset UpdatedAt { get; }
    }
}