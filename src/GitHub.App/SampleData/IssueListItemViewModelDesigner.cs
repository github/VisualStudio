using System;
using System.Diagnostics.CodeAnalysis;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class IssueListItemViewModelDesigner : ViewModelBase, IIssueListItemViewModel
    {
        public IActorViewModel Author { get; set; }
        public string NodeId { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public void CopyFrom(IIssueListItemViewModel other)
        {
            throw new NotImplementedException();
        }
    }
}