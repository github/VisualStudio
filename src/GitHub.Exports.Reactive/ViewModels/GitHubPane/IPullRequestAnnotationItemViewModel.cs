using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestAnnotationItemViewModel
    {
        CheckRunAnnotationModel Annotation { get; }
        bool IsExpanded { get; set; }
        string LineDescription { get; }
    }
}