using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.App.ViewModels.GitHubPane;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public sealed class PullRequestAnnotationsViewModelDesigner : PanePageViewModelBase, IPullRequestAnnotationsViewModel
    {
        public ILocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; } = 123;
        public string CheckRunId { get; set; }
        public ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }
        public string PullRequestTitle { get; } = "Fixing stuff in this PR";
        public string CheckSuiteName { get; } = "Awesome Check Suite";
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

        public Task InitializeAsync(ILocalRepositoryModel localRepository, IConnection connection, string owner, string repo,
            int pullRequestNumber, string checkRunId)
        {
            return Task.CompletedTask;
        }
    }
}