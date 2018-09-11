using System.Diagnostics.CodeAnalysis;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public sealed class PullRequestAnnotationItemViewModelDesigner : IPullRequestAnnotationItemViewModel
    {
        public CheckRunAnnotationModel Annotation { get; set; }
        public bool IsExpanded { get; set; }
        public string LineDescription => $"{Annotation.StartLine}:{Annotation.EndLine}";
    }
}