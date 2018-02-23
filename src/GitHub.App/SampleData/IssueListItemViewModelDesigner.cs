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
        public IssueListItemViewModelDesigner()
        {
            Labels = new[] { new IssueLabelModel { Name = "bug", Color = "#d73a4a" } };
        }

        public IActorViewModel Author { get; set; }
        public IReadOnlyList<IActorViewModel> Assignees { get; set; }
        public int CommentCount { get; set; }
        public IReadOnlyList<IssueLabelModel> Labels { get; set; }
        public string NodeId { get; set; }
        public int Number { get; set; }
        public IssueState State { get; set; }
        public string Title { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public void CopyFrom(IIssueListItemViewModel other)
        {
            throw new NotImplementedException();
        }
    }
}