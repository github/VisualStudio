using System.Reactive;
using System.Reactive.Linq;
using GitHub.Models;
using GitHub.Services;
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
        /// <param name="checkSuite">The check suite model.</param>
        /// <param name="session">The pull request session.</param>
        /// <param name="editorService">The pull request editor service.</param>
        public PullRequestAnnotationItemViewModel(
            CheckRunAnnotationModel annotation,
            bool isFileInPullRequest,
            CheckSuiteModel checkSuite,
            IPullRequestSession session,
            IPullRequestEditorService editorService)
        {
            Annotation = annotation;
            IsFileInPullRequest = isFileInPullRequest;

            OpenAnnotation = ReactiveCommand.CreateFromTask<Unit>(
                async _ => await editorService.OpenDiff(session, annotation.Path, checkSuite.HeadSha, annotation.EndLine - 1),
                Observable.Return(IsFileInPullRequest));
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