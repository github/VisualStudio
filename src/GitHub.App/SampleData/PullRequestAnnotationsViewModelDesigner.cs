using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public sealed class PullRequestAnnotationsViewModelDesigner : PanePageViewModelBase, IPullRequestAnnotationsViewModel
    {
        public LocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; } = 123;
        public string CheckRunId { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }
        public string PullRequestTitle { get; } = "Fixing stuff in this PR";
        public string CheckSuiteName { get; } = "Awesome Check Suite";
        public string CheckRunSummary { get; } = "Awesome Check Run Summary";
        public string CheckRunText { get; } = "Awesome Check Run Text";

        public IReadOnlyDictionary<string, IPullRequestAnnotationItemViewModel[]> AnnotationsDictionary { get; }
            = new Dictionary<string, IPullRequestAnnotationItemViewModel[]>
            {
                {
                    "asdf/asdf.cs",
                    new IPullRequestAnnotationItemViewModel[] 
                    {
                        new PullRequestAnnotationItemViewModelDesigner
                        {
                            Annotation = new CheckRunAnnotationModel
                            {
                                AnnotationLevel = CheckAnnotationLevel.Warning,
                                StartLine = 3,
                                EndLine = 4,
                                Path = "asdf/asdf.cs",
                                Message = "; is expected",
                                Title = "CS 12345"
                            },
                            IsExpanded = true,
                            IsFileInPullRequest = true
                        },
                        new PullRequestAnnotationItemViewModelDesigner
                        {
                            Annotation = new CheckRunAnnotationModel
                            {
                                AnnotationLevel = CheckAnnotationLevel.Failure,
                                StartLine = 3,
                                EndLine = 4,
                                Path = "asdf/asdf.cs",
                                Message = "; is expected",
                                Title = "CS 12345"
                            },
                            IsExpanded = true,
                            IsFileInPullRequest = true
                        },
                    }
                },
                {
                    "blah.cs",
                    new IPullRequestAnnotationItemViewModel[] 
                    {
                        new PullRequestAnnotationItemViewModelDesigner
                        {
                            Annotation = new CheckRunAnnotationModel
                            {
                                AnnotationLevel = CheckAnnotationLevel.Notice,
                                StartLine = 3,
                                EndLine = 4,
                                Path = "blah.cs",
                                Message = "; is expected",
                                Title = "CS 12345"
                            },
                            IsExpanded = true,
                        }
                    }
                },
            };

        public string CheckRunName { get; } = "Psuedo Check Run";

        public Task InitializeAsync(LocalRepositoryModel localRepository, IConnection connection, string owner,
            string repo,
            int pullRequestNumber, string checkRunId)
        {
            return Task.CompletedTask;
        }
    }
}