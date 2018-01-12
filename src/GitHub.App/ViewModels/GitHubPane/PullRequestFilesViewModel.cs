using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// View model displaying a tree of changed files in a pull request.
    /// </summary>
    [Export(typeof(IPullRequestFilesViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class PullRequestFilesViewModel : ViewModelBase, IPullRequestFilesViewModel
    {
        readonly IPullRequestService service;
        readonly BehaviorSubject<bool> isBranchCheckedOut = new BehaviorSubject<bool>(false);

        IPullRequestSession session;
        Func<IInlineCommentThreadModel, bool> commentFilter;
        int changedFilesCount;
        IReadOnlyList<IPullRequestChangeNode> items;
        CompositeDisposable subscriptions;

        [ImportingConstructor]
        public PullRequestFilesViewModel(
            IPullRequestService service,
            IPullRequestEditorService editorService)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(editorService, nameof(editorService));

            this.service = service;

            DiffFile = ReactiveCommand.CreateAsyncTask(x => 
                editorService.OpenDiff(session, ((IPullRequestFileNode)x).RelativePath, false));
            ViewFile = ReactiveCommand.CreateAsyncTask(x => 
                editorService.OpenFile(session, ((IPullRequestFileNode)x).RelativePath, false));
            DiffFileWithWorkingDirectory = ReactiveCommand.CreateAsyncTask(
                isBranchCheckedOut,
                x => editorService.OpenDiff(session, ((IPullRequestFileNode)x).RelativePath, true));
            OpenFileInWorkingDirectory = ReactiveCommand.CreateAsyncTask(
                isBranchCheckedOut,
                x => editorService.OpenFile(session, ((IPullRequestFileNode)x).RelativePath, true));

            OpenFirstComment = ReactiveCommand.CreateAsyncTask(async x =>
            {
                var file = (IPullRequestFileNode)x;
                var thread = await GetFirstCommentThread(file);

                if (thread != null)
                {
                    await editorService.OpenDiff(session, file.RelativePath, thread);
                }
            });
        }

        /// <inheritdoc/>
        public int ChangedFilesCount
        {
            get { return changedFilesCount; }
            private set { this.RaiseAndSetIfChanged(ref changedFilesCount, value); }
        }

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
            IPullRequestSession session,
            Func<IInlineCommentThreadModel, bool> commentFilter = null)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            subscriptions?.Dispose();
            this.session = session;
            this.commentFilter = commentFilter;
            subscriptions = new CompositeDisposable();
            subscriptions.Add(session.WhenAnyValue(x => x.IsCheckedOut).Subscribe(isBranchCheckedOut));

            var dirs = new Dictionary<string, PullRequestDirectoryNode>
            {
                { string.Empty, new PullRequestDirectoryNode(string.Empty) }
            };

            using (var changes = await service.GetTreeChanges(session.LocalRepository, session.PullRequest))
            {
                foreach (var changedFile in session.PullRequest.ChangedFiles)
                {
                    var node = new PullRequestFileNode(
                        session.LocalRepository.LocalPath,
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

                    var dir = GetDirectory(Path.GetDirectoryName(node.RelativePath), dirs);
                    dir.Files.Add(node);
                }
            }

            ChangedFilesCount = session.PullRequest.ChangedFiles.Count;
            Items = dirs[string.Empty].Children.ToList();
        }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> DiffFile { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> ViewFile { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> DiffFileWithWorkingDirectory { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> OpenFileInWorkingDirectory { get; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit> OpenFirstComment { get; }

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

        async Task<IInlineCommentThreadModel> GetFirstCommentThread(IPullRequestFileNode file)
        {
            var sessionFile = await session.GetFile(file.RelativePath);
            var threads = sessionFile.InlineCommentThreads.AsEnumerable();

            if (commentFilter != null)
            {
                threads = threads.Where(commentFilter);
            }

            return threads.FirstOrDefault();
        }
    }
}
