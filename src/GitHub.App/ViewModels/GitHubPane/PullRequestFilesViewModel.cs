using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// View model for a tree of changed files in a pull request.
    /// </summary>
    [Export(typeof(IPullRequestFilesViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class PullRequestFilesViewModel : ViewModelBase, IPullRequestFilesViewModel
    {
        IReadOnlyList<IPullRequestChangeNode> items;
        CompositeDisposable subscriptions;

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestChangeNode> Items
        {
            get { return items; }
            private set { this.RaiseAndSetIfChanged(ref items, value); }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            subscriptions?.Dispose();
            subscriptions = null;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            string repositoryPath,
            IPullRequestSession session,
            TreeChanges changes,
            Func<IInlineCommentThreadModel, bool> commentFilter = null)
        {
            if (subscriptions != null)
            {
                throw new NotSupportedException("PullRequestFilesViewModel is already initialized.");
            }

            subscriptions = new CompositeDisposable();

            var dirs = new Dictionary<string, PullRequestDirectoryNode>
            {
                { string.Empty, new PullRequestDirectoryNode(string.Empty) }
            };

            foreach (var changedFile in session.PullRequest.ChangedFiles)
            {
                var node = new PullRequestFileNode(
                    repositoryPath,
                    changedFile.FileName,
                    changedFile.Sha,
                    changedFile.Status,
                    GetOldFileName(changedFile, changes));
                var file = await session.GetFile(changedFile.FileName);

                if (file != null)
                {
                    subscriptions.Add(file.WhenAnyValue(x => x.InlineCommentThreads)
                        .Subscribe(x => node.CommentCount = CountComments(x, commentFilter)));
                }

                var dir = GetDirectory(node.DirectoryPath, dirs);
                dir.Files.Add(node);
            }

            Items = dirs[string.Empty].Children.ToList();
        }

        /// <inheritdoc/>
        public ReactiveCommand<object> DiffFile { get; } = ReactiveCommand.Create();

        /// <inheritdoc/>
        public ReactiveCommand<object> ViewFile { get; } = ReactiveCommand.Create();

        /// <inheritdoc/>
        public ReactiveCommand<object> DiffFileWithWorkingDirectory { get; } = ReactiveCommand.Create();

        /// <inheritdoc/>
        public ReactiveCommand<object> OpenFileInWorkingDirectory { get; } = ReactiveCommand.Create();

        static int CountComments(
            IEnumerable<IInlineCommentThreadModel> thread,
            Func<IInlineCommentThreadModel, bool> commentFilter)
        {
            return thread.Count(x => x.LineNumber != -1 && (commentFilter?.Invoke(x) ?? true));
        }

        static PullRequestDirectoryNode GetDirectory(string path, Dictionary<string, PullRequestDirectoryNode> dirs)
        {
            PullRequestDirectoryNode dir;

            if (!dirs.TryGetValue(path, out dir))
            {
                var parentPath = Path.GetDirectoryName(path);
                var parentDir = GetDirectory(parentPath, dirs);

                dir = new PullRequestDirectoryNode(path);

                if (!parentDir.Directories.Any(x => x.DirectoryName == dir.DirectoryName))
                {
                    parentDir.Directories.Add(dir);
                    dirs.Add(path, dir);
                }
            }

            return dir;
        }

        static string GetOldFileName(IPullRequestFileModel file, TreeChanges changes)
        {
            if (file.Status == PullRequestFileStatus.Renamed)
            {
                var fileName = file.FileName.Replace("/", "\\");
                return changes?.Renamed.FirstOrDefault(x => x.Path == fileName)?.OldPath;
            }

            return null;
        }
    }
}
