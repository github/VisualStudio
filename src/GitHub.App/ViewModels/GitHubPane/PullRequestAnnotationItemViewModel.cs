using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.App.ViewModels.GitHubPane
{
    /// <summary>
    /// The viewmodel for a single annotation item in a list
    /// </summary>
    public class PullRequestAnnotationItemViewModel : ViewModelBase, IPullRequestAnnotationItemViewModel
    {
        bool isExpanded;

        public PullRequestAnnotationItemViewModel(CheckRunAnnotationModel annotation)
        {
            this.Annotation = annotation;
        }

        /// <summary>
        /// Gets the annotation model.
        /// </summary>
        public CheckRunAnnotationModel Annotation { get; }

        /// <summary>
        /// Gets a formatted descriptor of the line(s) the annotation is about.
        /// </summary>
        public string LineDescription => $"{Annotation.StartLine}:{Annotation.EndLine}";

        /// <summary>
        /// Gets or sets a flag to control the expanded state.
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { this.RaiseAndSetIfChanged(ref isExpanded, value); }
        }
    }
}