using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class IssueListItemViewModelDesigner : ViewModelBase, IIssueListItemViewModel
    {
        public IActorViewModel Author { get; set; }
        public List<IActorViewModel> Assignees { get; set; }
        public int CommentCount { get; set; }
        public List<ILabelViewModel> Labels { get; set; }
        public string NodeId { get; set; }
        public int Number { get; set; }
        public IssueState State { get; set; }
        public string Title { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        IReadOnlyList<IActorViewModel> IIssueListItemViewModel.Assignees => Assignees;
        IReadOnlyList<ILabelViewModel> IIssueListItemViewModel.Labels => Labels;

        public void CopyFrom(IIssueListItemViewModel other)
        {
            throw new NotImplementedException();
        }
    }
}