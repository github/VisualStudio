using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.App.ViewModels.GitHubPane
{
    /// <inheritdoc cref="IPullRequestAnnotationItemViewModel"/>
    public class PullRequestAnnotationItemViewModel : ViewModelBase, IPullRequestAnnotationItemViewModel
    {
        bool isExpanded;

        /// <summary>
        /// Initializes the <see cref="PullRequestAnnotationItemViewModel"/>.
        /// </summary>
        /// <param name="annotation">The check run annotation model.</param>
        public PullRequestAnnotationItemViewModel(CheckRunAnnotationModel annotation)
        {
            this.Annotation = annotation;

            OpenAnnotation = ReactiveCommand.Create(() => { });
        }

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