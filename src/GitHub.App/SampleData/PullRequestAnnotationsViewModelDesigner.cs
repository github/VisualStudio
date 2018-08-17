using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GitHub.App.ViewModels.GitHubPane;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public sealed class PullRequestAnnotationsViewModelDesigner : PanePageViewModelBase, IPullRequestAnnotationsViewModel
    {
        public ILocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; } = 123;
        public int CheckRunId { get; set; }
        public ReactiveCommand<object> NavigateToPullRequest { get; }
        public string PullRequestTitle { get; } = "Fixing stuff in this PR";
        public string CheckRunName { get; } = "Psuedo Check Run";
        public IReadOnlyList<IPullRequestAnnotationItemViewModel> Annotations { get; } = new[]
        {
            new PullRequestAnnotationItemViewModel(new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Notice,
                StartLine = 3,
                EndLine = 4,
                Filename = "asdf/asdf.cs",
                Message = "; is expected",
                Title = "CS 12345"
            }),
            new PullRequestAnnotationItemViewModel(new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Warning,
                StartLine = 3,
                EndLine = 4,
                Filename = "asdf/asdf.cs",
                Message = "; is expected",
                Title = "CS 12345"
            }),
            new PullRequestAnnotationItemViewModel(new CheckRunAnnotationModel
            {
                AnnotationLevel = CheckAnnotationLevel.Failure,
                StartLine = 3,
                EndLine = 4,
                Filename = "blah.cs",
                Message = "; is expected",
                Title = "CS 12345"
            })
        };

        public Task InitializeAsync(ILocalRepositoryModel localRepository, IConnection connection, string owner, string repo,
            int pullRequestNumber, int checkRunId)
        {
            return Task.CompletedTask;
        }
    }
}