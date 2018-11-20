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

        /// <summary>
        /// Gets a flag which indicates this annotation item is from a file changed in this pull request.
        /// </summary>
        bool IsFileInPullRequest { get; }

        /// <summary>
        /// Gets a command which opens the annotation in the diff view.
        /// </summary>
        ReactiveCommand<Unit, Unit> OpenAnnotation { get; }
    }
}