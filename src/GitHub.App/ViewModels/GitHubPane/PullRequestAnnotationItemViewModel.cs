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
    /// <summary>
    /// The viewmodel for a single annotation item in a list
    /// </summary>
    public class PullRequestAnnotationItemViewModel : ViewModelBase, IPullRequestAnnotationItemViewModel
    {
        readonly CheckSuiteModel checkSuite;
        readonly CheckRunModel checkRun;
        readonly IPullRequestSession session;
        readonly IPullRequestEditorService editorService;

        bool isExpanded;

        public PullRequestAnnotationItemViewModel(CheckSuiteModel checkSuite, 
            CheckRunModel checkRun,
            CheckRunAnnotationModel annotation,
            IPullRequestSession session,
            IPullRequestEditorService editorService)
        {
            this.checkSuite = checkSuite;
            this.checkRun = checkRun;
            this.session = session;
            this.editorService = editorService;
            this.Annotation = annotation;

            IsFileInPullRequest = session.PullRequest.ChangedFiles.Any(model => model.FileName == annotation.Path);

            OpenAnnotation = ReactiveCommand.CreateAsyncTask(Observable.Return(IsFileInPullRequest), async x =>
            {
                await editorService.OpenDiff(session, annotation.Path, checkSuite.HeadSha, annotation.EndLine - 1);
            });
        }

        public bool IsFileInPullRequest { get; }

        /// <summary>
        /// Gets the annotation model.
        /// </summary>
        public CheckRunAnnotationModel Annotation { get; }

        /// <summary>
        /// Gets a formatted descriptor of the line(s) the annotation is about.
        /// </summary>
        public string LineDescription => $"{Annotation.StartLine}:{Annotation.EndLine}";

        public ReactiveCommand<Unit> OpenAnnotation { get; }

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