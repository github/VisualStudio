using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
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

        IPullRequestSession pullRequestSession;
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

            DiffFile = ReactiveCommand.CreateFromTask<IPullRequestFileNode>(x =>
                editorService.OpenDiff(pullRequestSession, x.RelativePath, "HEAD"));
            ViewFile = ReactiveCommand.CreateFromTask<IPullRequestFileNode>(x =>
                editorService.OpenFile(pullRequestSession, x.RelativePath, false));
            DiffFileWithWorkingDirectory = ReactiveCommand.CreateFromTask<IPullRequestFileNode>(
                x => editorService.OpenDiff(pullRequestSession, x.RelativePath),
                isBranchCheckedOut);
            OpenFileInWorkingDirectory = ReactiveCommand.CreateFromTask<IPullRequestFileNode>(
                x => editorService.OpenFile(pullRequestSession, x.RelativePath, true),
                isBranchCheckedOut);

            OpenFirstComment = ReactiveCommand.CreateFromTask<IPullRequestFileNode>(async file =>
            {
                var thread = await GetFirstCommentThread(file);

                if (thread != null)
                {
                    await editorService.OpenDiff(pullRequestSession, file.RelativePath, thread);
                }
            });

            OpenFirstAnnotationNotice = ReactiveCommand.CreateFromTask<IPullRequestFileNode>(
                async file => await OpenFirstAnnotation(editorService, file, CheckAnnotationLevel.Notice));

            OpenFirstAnnotationWarning = ReactiveCommand.CreateFromTask<IPullRequestFileNode>(
                async file => await OpenFirstAnnotation(editorService, file, CheckAnnotationLevel.Warning));

            OpenFirstAnnotationFailure = ReactiveCommand.CreateFromTask<IPullRequestFileNode>(
                async file => await OpenFirstAnnotation(editorService, file, CheckAnnotationLevel.Failure));
        }

        private async Task OpenFirstAnnotation(IPullRequestEditorService editorService, IPullRequestFileNode file,
            CheckAnnotationLevel checkAnnotationLevel)
        {
            var annotationModel = await GetFirstAnnotation(file, checkAnnotationLevel);

            if (annotationModel != null)
            {
                //AnnotationModel.EndLine is a 1-based number
                //EditorService.OpenDiff takes a 0-based line number to start searching AFTER and will open the next tag
                var nextInlineCommentFromLine = annotationModel.EndLine - 2;
                await editorService.OpenDiff(pullRequestSession, file.RelativePath, annotationModel.HeadSha, nextInlineCommentFromLine);
            }
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
            Func<IInlineCommentThreadModel, bool> filter = null)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            subscriptions?.Dispose();
            this.pullRequestSession = session;
            this.commentFilter = filter;
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
                            .Subscribe(x => node.CommentCount = CountComments(x, filter)));

                        subscriptions.Add(file.WhenAnyValue(x => x.InlineAnnotations)
                            .Subscribe(x =>
                            {
                                var noticeCount = x.Count(model => model.AnnotationLevel == CheckAnnotationLevel.Notice);
                                var warningCount = x.Count(model => model.AnnotationLevel == CheckAnnotationLevel.Warning);
                                var failureCount = x.Count(model => model.AnnotationLevel == CheckAnnotationLevel.Failure);

                                node.AnnotationNoticeCount = noticeCount;
                                node.AnnotationWarningCount = warningCount;
                                node.AnnotationFailureCount = failureCount;
                            }));
                    }

                    var dir = GetDirectory(Path.GetDirectoryName(node.RelativePath), dirs);
                    dir.Files.Add(node);
                }
            }

            ChangedFilesCount = session.PullRequest.ChangedFiles.Count;
            Items = dirs[string.Empty].Children.ToList();
        }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestFileNode, Unit> DiffFile { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestFileNode, Unit> ViewFile { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestFileNode, Unit> DiffFileWithWorkingDirectory { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFileInWorkingDirectory { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstComment { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationNotice { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationWarning { get; }

        /// <inheritdoc/>
        public ReactiveCommand<IPullRequestFileNode, Unit> OpenFirstAnnotationFailure { get; }

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

        static string GetOldFileName(PullRequestFileModel file, TreeChanges changes)
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
            var sessionFile = await pullRequestSession.GetFile(file.RelativePath);
            var threads = sessionFile.InlineCommentThreads.AsEnumerable();

            if (commentFilter != null)
            {
                threads = threads.Where(commentFilter);
            }

            return threads.FirstOrDefault();
        }

        async Task<InlineAnnotationModel> GetFirstAnnotation(IPullRequestFileNode file,
            CheckAnnotationLevel annotationLevel)
        {
            var sessionFile = await pullRequestSession.GetFile(file.RelativePath);
            var annotations = sessionFile.InlineAnnotations;

            return annotations.OrderBy(model => model.EndLine).FirstOrDefault(model => model.AnnotationLevel == annotationLevel);
        }

        /// <summary>
        /// Implements the <see cref="OpenFileInWorkingDirectory"/> command.
        /// </summary>
        /// <remarks>
        /// We need to "Open File in Solution" when the parameter passed to the command parameter
        /// represents a deleted file. ReactiveCommand doesn't allow us to change the CanExecute
        /// state depending on the parameter, so we override 
        /// <see cref="ICommand.CanExecute(object)"/> to do this ourselves.
        /// </remarks>
        class NonDeletedFileCommand : ReactiveCommand<Unit, Unit>, ICommand
        {
            public NonDeletedFileCommand(
                IObservable<bool> canExecute,
                Func<object, Task> executeAsync)
                : base(x => executeAsync(x).ToObservable(), canExecute, null)
            {
            }

            bool ICommand.CanExecute(object parameter)
            {
                if (parameter is IPullRequestFileNode node)
                {
                    if (node.Status == PullRequestFileStatus.Removed)
                    {
                        return false;
                    }
                }

                return true; ////CanExecute(parameter);
            }
        }
    }
}
