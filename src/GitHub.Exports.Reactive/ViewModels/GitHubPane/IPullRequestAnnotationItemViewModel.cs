using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// The viewmodel for a single annotation item in a list
    /// </summary>
    public interface IPullRequestAnnotationItemViewModel
    {
        /// <summary>
        /// Gets the annotation model.
        /// </summary>
        CheckRunAnnotationModel Annotation { get; }

        /// <summary>
        /// Gets a formatted descriptor of the line(s) the annotation is about.
        /// </summary>
        string LineDescription { get; }

        /// <summary>
        /// Gets or sets a flag to control the expanded state.
        /// </summary>
        bool IsExpanded { get; set; }

        bool IsFileInPullRequest { get; }
        ReactiveCommand<Unit> OpenAnnotation { get; }
    }
}