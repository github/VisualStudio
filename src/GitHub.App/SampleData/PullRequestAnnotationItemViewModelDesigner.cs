using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public sealed class PullRequestAnnotationItemViewModelDesigner : IPullRequestAnnotationItemViewModel
    {
        public CheckRunAnnotationModel Annotation { get; set; }
        public bool IsExpanded { get; set; }
        public string LineDescription => $"{Annotation.StartLine}:{Annotation.EndLine}";
        public bool IsFileInPullRequest { get; set;  }
        public ReactiveCommand<Unit, Unit> OpenAnnotation { get; }
    }
}