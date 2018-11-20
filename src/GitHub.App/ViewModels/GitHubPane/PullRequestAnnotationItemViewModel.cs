using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <inheritdoc cref="IPullRequestAnnotationItemViewModel"/>
    public class PullRequestAnnotationItemViewModel : ViewModelBase, IPullRequestAnnotationItemViewModel
    {
        bool isExpanded;

        /// <summary>
        /// Initializes the <see cref="PullRequestAnnotationItemViewModel"/>.
        /// </summary>
        /// <param name="annotation">The check run annotation model.</param>
        /// <param name="isFileInPullRequest">A flag that denotes if the annotation is part of the pull request's changes.</param>
        public PullRequestAnnotationItemViewModel(CheckRunAnnotationModel annotation, bool isFileInPullRequest)
        {
            Annotation = annotation;
            IsFileInPullRequest = isFileInPullRequest;

            OpenAnnotation = ReactiveCommand.Create(() => { });
        }

        /// <inheritdoc />
        public bool IsFileInPullRequest { get; }

        /// <inheritdoc />
        public CheckRunAnnotationModel Annotation { get; }

        /// <inheritdoc />
        public string LineDescription => $"{Annotation.StartLine}:{Annotation.EndLine}";

        /// <inheritdoc />
        public ReactiveCommand<Unit, Unit> OpenAnnotation { get; }

        /// <inheritdoc />
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { this.RaiseAndSetIfChanged(ref isExpanded, value); }
        }
    }
}