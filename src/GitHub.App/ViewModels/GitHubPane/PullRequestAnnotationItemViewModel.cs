using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestAnnotationItemViewModel : ViewModelBase, IPullRequestAnnotationItemViewModel
    {
        bool isExpanded;

        public PullRequestAnnotationItemViewModel(CheckRunAnnotationModel model)
        {
            this.Model = model;
        }

        public CheckRunAnnotationModel Model { get; }

        public string LineDescription => $"{Model.StartLine}:{Model.EndLine}";

        public bool IsExpanded
        {
            get { return isExpanded; }
            set { this.RaiseAndSetIfChanged(ref isExpanded, value); }
        }
    }
}