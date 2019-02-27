using System.Reactive;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class PullRequestReviewFileCommentViewModelDesigner : IPullRequestReviewFileCommentViewModel
    {
        public string Body { get; set; }
        public string RelativePath { get; set; }
        public ReactiveCommand<Unit, Unit> Open { get; }
    }
}
