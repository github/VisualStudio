using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestListItemViewModelDesigner : ViewModelBase, IPullRequestListItemViewModel
    {
        public string Id { get; set; }
        public IActorViewModel Author { get; set; }
        public int CommentCount { get; set; }
        public IReadOnlyList<ILabelViewModel> Labels { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
