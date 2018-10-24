using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    public class PullRequestFilesViewModelDesigner : PanePageViewModelBase, IPullRequestFilesViewModel
    {
        public PullRequestFilesViewModelDesigner()
        {
            Items = new[]
            {
                new PullRequestDirectoryNode("src")
                {
                    Files =
                    {
                        new PullRequestFileNode("x", "src/File1.cs", "x", PullRequestFileStatus.Added, null),
                        new PullRequestFileNode("x", "src/File2.cs", "x", PullRequestFileStatus.Modified, null),
                        new PullRequestFileNode("x", "src/File3.cs", "x", PullRequestFileStatus.Removed, null),
                        new PullRequestFileNode("x", "src/File4.cs", "x", PullRequestFileStatus.Renamed, "src/Old.cs"),
                    }
                }
            };
            ChangedFilesCount = 4;
        }

        public int ChangedFilesCount { get; set; }
        public IReadOnlyList<IPullRequestChangeNode> Items { get; }
        public ReactiveCommand<IPullRequestFileNode, Unit> DiffFile { get; }
        public ReactiveCommand<IPullRequestFileNode, Unit> ViewFile { get; }
        public ReactiveCommand<IPullRequestFileNode, Unit> DiffFileWithWorkingDirectory { get; }
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFileInWorkingDirectory { get; }
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstComment { get; }
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationNotice { get; }
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationWarning { get; }
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationFailure { get; }

        public Task InitializeAsync(
            IPullRequestSession session,
            Func<IInlineCommentThreadModel, bool> commentFilter = null)
        {
            return Task.CompletedTask;
        }
    }
}
