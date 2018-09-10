using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestAnnotationItemViewModel
    {
        CheckRunAnnotationModel Annotation { get; }
        bool IsExpanded { get; set; }
        string LineDescription { get; }
        bool IsFileInPullRequest { get; }
        ReactiveCommand<Unit> OpenAnnotation { get; }
    }
}