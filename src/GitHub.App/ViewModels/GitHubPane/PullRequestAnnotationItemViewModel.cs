using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestAnnotationItemViewModel : ViewModelBase, IPullRequestAnnotationItemViewModel
    {
        bool isExpanded;

        public PullRequestAnnotationItemViewModel(CheckRunAnnotationModel annotation)
        {
            this.Annotation = annotation;
        }

        public CheckRunAnnotationModel Annotation { get; }

        public string LineDescription => $"{Annotation.StartLine}:{Annotation.EndLine}";

        public bool IsExpanded
        {
            get { return isExpanded; }
            set { this.RaiseAndSetIfChanged(ref isExpanded, value); }
        }
    }
}