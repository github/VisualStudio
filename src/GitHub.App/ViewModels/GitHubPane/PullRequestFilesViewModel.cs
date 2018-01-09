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
using EnvDTE;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using ReactiveUI;
using Task = System.Threading.Tasks.Task;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// View model for a tree of changed files in a pull request.
    /// </summary>
    [Export(typeof(IPullRequestFilesViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class PullRequestFilesViewModel : ViewModelBase, IPullRequestFilesViewModel
    {
        readonly IGitHubServiceProvider serviceProvider;
        readonly IPullRequestService pullRequestService;
        readonly IVsEditorAdaptersFactoryService vsEditorAdaptersFactory;
        readonly IStatusBarNotificationService statusBar;
        readonly IUsageTracker usageTracker;
        readonly BehaviorSubject<bool> isBranchCheckedOut = new BehaviorSubject<bool>(false);
        IPullRequestSession session;
        IReadOnlyList<IPullRequestChangeNode> items;
        CompositeDisposable subscriptions;

        [ImportingConstructor]
        public PullRequestFilesViewModel(
            IGitHubServiceProvider serviceProvider,
            IPullRequestService pullRequestService,
            IVsEditorAdaptersFactoryService vsEditorAdaptersFactory,
            IStatusBarNotificationService statusBar,
            IUsageTracker usageTracker)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(pullRequestService, nameof(pullRequestService));
            Guard.ArgumentNotNull(vsEditorAdaptersFactory, nameof(vsEditorAdaptersFactory));
            Guard.ArgumentNotNull(statusBar, nameof(statusBar));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));

            this.serviceProvider = serviceProvider;
            this.pullRequestService = pullRequestService;
            this.vsEditorAdaptersFactory = vsEditorAdaptersFactory;
            this.statusBar = statusBar;
            this.usageTracker = usageTracker;

            DiffFile = ReactiveCommand.CreateAsyncTask(x => DoDiffFile((IPullRequestFileNode)x, false));
            ViewFile = ReactiveCommand.CreateAsyncTask(x => DoOpenFile((IPullRequestFileNode)x, false));

            DiffFileWithWorkingDirectory = ReactiveCommand.CreateAsyncTask(
                isBranchCheckedOut,
                x => DoDiffFile((IPullRequestFileNode)x, true));
            OpenFileInWorkingDirectory = ReactiveCommand.CreateAsyncTask(
                isBranchCheckedOut,
                x => DoOpenFile((IPullRequestFileNode)x, true));
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
            TreeChanges changes,
            Func<IInlineCommentThreadModel, bool> commentFilter = null)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(changes, nameof(changes));

            if (this.session != null)
            {
                throw new NotSupportedException("PullRequestFilesViewModel is already initialized.");
            }

            this.session = session;
            subscriptions = new CompositeDisposable();
            subscriptions.Add(session.WhenAnyValue(x => x.IsCheckedOut).Subscribe(isBranchCheckedOut));

            var dirs = new Dictionary<string, PullRequestDirectoryNode>
            {
                { string.Empty, new PullRequestDirectoryNode(string.Empty) }
            };

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

                var dir = GetDirectory(node.DirectoryPath, dirs);
                dir.Files.Add(node);
            }

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

        async Task DoDiffFile(IPullRequestFileNode file, bool workingDirectory)
        {
            try
            {
                var rightPath = System.IO.Path.Combine(file.DirectoryPath, file.FileName);
                var leftPath = file.OldPath ?? rightPath;
                var rightFile = workingDirectory ? GetLocalFilePath(file) : await ExtractFile(file, true);
                var leftFile = await ExtractFile(file, false);
                var leftLabel = $"{leftPath};{session.GetBaseBranchDisplay()}";
                var rightLabel = workingDirectory ? rightPath : $"{rightPath};PR {session.PullRequest.Number}";
                var caption = $"Diff - {file.FileName}";
                var options = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_DetectBinaryFiles |
                    __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary;

                if (!workingDirectory)
                {
                    options |= __VSDIFFSERVICEOPTIONS.VSDIFFOPT_RightFileIsTemporary;
                }

                IVsWindowFrame frame;
                using (OpenInProvisionalTab())
                {
                    var tooltip = $"{leftLabel}\nvs.\n{rightLabel}";

                    // Diff window will open in provisional (right hand) tab until document is touched.
                    frame = VisualStudio.Services.DifferenceService.OpenComparisonWindow2(
                        leftFile,
                        rightFile,
                        caption,
                        tooltip,
                        leftLabel,
                        rightLabel,
                        string.Empty,
                        string.Empty,
                        (uint)options);
                }

                object docView;
                frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);
                var diffViewer = ((IVsDifferenceCodeWindow)docView).DifferenceViewer;

                AddBufferTag(diffViewer.LeftView.TextBuffer, session, leftPath, DiffSide.Left);

                if (!workingDirectory)
                {
                    AddBufferTag(diffViewer.RightView.TextBuffer, session, rightPath, DiffSide.Right);
                }

                if (workingDirectory)
                    await usageTracker.IncrementCounter(x => x.NumberOfPRDetailsCompareWithSolution);
                else
                    await usageTracker.IncrementCounter(x => x.NumberOfPRDetailsViewChanges);
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error opening file", e);
            }
        }

        async Task DoOpenFile(IPullRequestFileNode file, bool workingDirectory)
        {
            try
            {
                var fullPath = GetLocalFilePath(file);
                var fileName = workingDirectory ? fullPath : await ExtractFile(file, true);

                using (workingDirectory ? null : OpenInProvisionalTab())
                {
                    var window = VisualStudio.Services.Dte.ItemOperations.OpenFile(fileName);
                    window.Document.ReadOnly = !workingDirectory;

                    var buffer = GetBufferAt(fileName);

                    if (!workingDirectory)
                    {
                        AddBufferTag(buffer, session, fullPath, null);
                    }
                }

                if (workingDirectory)
                    await usageTracker.IncrementCounter(x => x.NumberOfPRDetailsOpenFileInSolution);
                else
                    await usageTracker.IncrementCounter(x => x.NumberOfPRDetailsViewFile);
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error opening file", e);
            }
        }

        void AddBufferTag(ITextBuffer buffer, IPullRequestSession session, string path, DiffSide? side)
        {
            buffer.Properties.GetOrCreateSingletonProperty(
                typeof(PullRequestTextBufferInfo),
                () => new PullRequestTextBufferInfo(session, path, side));

            var projection = buffer as IProjectionBuffer;

            if (projection != null)
            {
                foreach (var source in projection.SourceBuffers)
                {
                    AddBufferTag(source, session, path, side);
                }
            }
        }

        Task<string> ExtractFile(IPullRequestFileNode file, bool head)
        {
            var relativePath = Path.Combine(file.DirectoryPath, file.FileName);
            var encoding = pullRequestService.GetEncoding(session.LocalRepository, relativePath);

            if (!head && file.OldPath != null)
            {
                relativePath = file.OldPath;
            }

            return pullRequestService.ExtractFile(
                session.LocalRepository,
                session.PullRequest,
                relativePath,
                head,
                encoding).ToTask();
        }

        ITextBuffer GetBufferAt(string filePath)
        {
            IVsUIHierarchy uiHierarchy;
            uint itemID;
            IVsWindowFrame windowFrame;

            if (VsShellUtilities.IsDocumentOpen(
                    serviceProvider,
                    filePath,
                    Guid.Empty,
                    out uiHierarchy,
                    out itemID,
                    out windowFrame))
            {
                IVsTextView view = VsShellUtilities.GetTextView(windowFrame);
                IVsTextLines lines;
                if (view.GetBuffer(out lines) == 0)
                {
                    var buffer = lines as IVsTextBuffer;
                    if (buffer != null)
                        return vsEditorAdaptersFactory.GetDataBuffer(buffer);
                }
            }

            return null;
        }

        string GetLocalFilePath(IPullRequestFileNode file)
        {
            return Path.Combine(session.LocalRepository.LocalPath, file.DirectoryPath, file.FileName);
        }

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

        static IDisposable OpenInProvisionalTab()
        {
            return new NewDocumentStateScope(
                __VSNEWDOCUMENTSTATE.NDS_Provisional,
                VSConstants.NewDocumentStateReason.SolutionExplorer);
        }

        void ShowErrorInStatusBar(string message, Exception e)
        {
            statusBar.ShowMessage(message + ": " + e.Message);
        }
    }
}
