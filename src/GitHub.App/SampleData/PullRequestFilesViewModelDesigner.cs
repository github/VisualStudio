using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using LibGit2Sharp;
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
        }

        public ReactiveCommand<object> DiffFile { get; }
        public ReactiveCommand<object> ViewFile { get; }
        public ReactiveCommand<object> DiffFileWithWorkingDirectory { get; }
        public ReactiveCommand<object> OpenFileInWorkingDirectory { get; }

        public IReadOnlyList<IPullRequestChangeNode> Items { get; }

        public Task InitializeAsync(
            string repositoryPath,
            IPullRequestSession session,
            TreeChanges changes,
            Func<IInlineCommentThreadModel, bool> commentFilter = null)
        {
            return Task.CompletedTask;
        }
    }
}
