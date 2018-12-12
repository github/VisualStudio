using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EnvDTE;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Models.Drafts;
using GitHub.ViewModels;
using GitHub.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services
{
    /// <summary>
    /// Services for opening views of pull request files in Visual Studio.
    /// </summary>
    [Export(typeof(IPullRequestEditorService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestEditorService : IPullRequestEditorService
    {
        // If the target line doesn't have a unique match, search this number of lines above looking for a match.
        public const int MatchLinesAboveTarget = 4;

        readonly IGitHubServiceProvider serviceProvider;
        readonly IPullRequestService pullRequestService;
        readonly IVsEditorAdaptersFactoryService vsEditorAdaptersFactory;
        readonly IStatusBarNotificationService statusBar;
        readonly IGoToSolutionOrPullRequestFileCommand goToSolutionOrPullRequestFileCommand;
        readonly IEditorOptionsFactoryService editorOptionsFactoryService;
        readonly IMessageDraftStore draftStore;
        readonly IInlineCommentPeekService peekService;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public PullRequestEditorService(
            IGitHubServiceProvider serviceProvider,
            IPullRequestService pullRequestService,
            IVsEditorAdaptersFactoryService vsEditorAdaptersFactory,
            IStatusBarNotificationService statusBar,
            IGoToSolutionOrPullRequestFileCommand goToSolutionOrPullRequestFileCommand,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            IMessageDraftStore draftStore,
            IInlineCommentPeekService peekService,
            IUsageTracker usageTracker)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(pullRequestService, nameof(pullRequestService));
            Guard.ArgumentNotNull(vsEditorAdaptersFactory, nameof(vsEditorAdaptersFactory));
            Guard.ArgumentNotNull(statusBar, nameof(statusBar));
            Guard.ArgumentNotNull(goToSolutionOrPullRequestFileCommand, nameof(goToSolutionOrPullRequestFileCommand));
            Guard.ArgumentNotNull(goToSolutionOrPullRequestFileCommand, nameof(editorOptionsFactoryService));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));
            Guard.ArgumentNotNull(peekService, nameof(peekService));
            Guard.ArgumentNotNull(draftStore, nameof(draftStore));

            this.serviceProvider = serviceProvider;
            this.pullRequestService = pullRequestService;
            this.vsEditorAdaptersFactory = vsEditorAdaptersFactory;
            this.statusBar = statusBar;
            this.goToSolutionOrPullRequestFileCommand = goToSolutionOrPullRequestFileCommand;
            this.editorOptionsFactoryService = editorOptionsFactoryService;
            this.draftStore = draftStore;
            this.peekService = peekService;
            this.usageTracker = usageTracker;
        }

        /// <inheritdoc/>
        public async Task<ITextView> OpenFile(
            IPullRequestSession session,
            string relativePath,
            bool workingDirectory)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotEmptyString(relativePath, nameof(relativePath));

            try
            {
                var fullPath = GetAbsolutePath(session.LocalRepository, relativePath);
                string fileName;
                string commitSha;

                if (workingDirectory)
                {
                    fileName = fullPath;
                    commitSha = null;
                }
                else
                {
                    var file = await session.GetFile(relativePath);
                    fileName = await pullRequestService.ExtractToTempFile(
                        session.LocalRepository,
                        session.PullRequest,
                        file.RelativePath,
                        file.CommitSha,
                        pullRequestService.GetEncoding(session.LocalRepository, file.RelativePath));
                    commitSha = file.CommitSha;
                }

                IVsTextView textView;
                IWpfTextView wpfTextView;
                using (workingDirectory ? null : OpenInProvisionalTab())
                {
                    var readOnly = !workingDirectory;
                    textView = OpenDocument(fileName, readOnly, out wpfTextView);

                    if (!workingDirectory)
                    {
                        AddBufferTag(wpfTextView.TextBuffer, session, fullPath, commitSha, null);
                        EnableNavigateToEditor(textView, session);
                    }
                }

                if (workingDirectory)
                    await usageTracker.IncrementCounter(x => x.NumberOfPRDetailsOpenFileInSolution);
                else
                    await usageTracker.IncrementCounter(x => x.NumberOfPRDetailsViewFile);

                return wpfTextView;
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error opening file", e);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IDifferenceViewer> OpenDiff(IPullRequestSession session, string relativePath, string headSha, bool scrollToFirstDraftOrDiff)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotEmptyString(relativePath, nameof(relativePath));

            try
            {
                var workingDirectory = headSha == null;
                var file = await session.GetFile(relativePath, headSha ?? "HEAD");
                var mergeBase = await pullRequestService.GetMergeBase(session.LocalRepository, session.PullRequest);
                var encoding = pullRequestService.GetEncoding(session.LocalRepository, file.RelativePath);
                var rightFile = workingDirectory ?
                    Path.Combine(session.LocalRepository.LocalPath, relativePath) :
                    await pullRequestService.ExtractToTempFile(
                        session.LocalRepository,
                        session.PullRequest,
                        relativePath,
                        file.CommitSha,
                        encoding);

                var diffViewer = FocusExistingDiffViewer(session, mergeBase, rightFile);
                if (diffViewer != null)
                {
                    return diffViewer;
                }

                var leftFile = await pullRequestService.ExtractToTempFile(
                    session.LocalRepository,
                    session.PullRequest,
                    relativePath,
                    mergeBase,
                    encoding);
                var leftPath = await GetBaseFileName(session, file);
                var rightPath = file.RelativePath;
                var leftLabel = $"{leftPath};{session.GetBaseBranchDisplay()}";
                var rightLabel = workingDirectory ? rightPath : $"{rightPath};PR {session.PullRequest.Number}";
                var caption = $"Diff - {Path.GetFileName(file.RelativePath)}";
                var options = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_DetectBinaryFiles |
                    __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary;
                var openThread = (line: -1, side: DiffSide.Left);
                var scrollToFirstDiff = false;

                if (!workingDirectory)
                {
                    options |= __VSDIFFSERVICEOPTIONS.VSDIFFOPT_RightFileIsTemporary;
                }

                if (scrollToFirstDraftOrDiff)
                {
                    var (key, _) = PullRequestReviewCommentThreadViewModel.GetDraftKeys(
                        session.LocalRepository.CloneUrl.WithOwner(session.RepositoryOwner),
                        session.PullRequest.Number,
                        relativePath,
                        0);
                    var drafts = (await draftStore.GetDrafts<PullRequestReviewCommentDraft>(key)
                        .ConfigureAwait(true))
                        .OrderByDescending(x => x.data.UpdatedAt)
                        .ToList();

                    if (drafts.Count > 0 && int.TryParse(drafts[0].secondaryKey, out var line))
                    {
                        openThread = (line, drafts[0].data.Side);
                        scrollToFirstDiff = false;
                    }
                    else
                    {
                        scrollToFirstDiff = true;
                    }
                }

                IVsWindowFrame frame;
                using (OpenWithOption(DifferenceViewerOptions.ScrollToFirstDiffName, scrollToFirstDiff))
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

                diffViewer = GetDiffViewer(frame);

                var leftText = diffViewer.LeftView.TextBuffer.CurrentSnapshot.GetText();
                var rightText = diffViewer.RightView.TextBuffer.CurrentSnapshot.GetText();
                if (leftText == string.Empty)
                {
                    // Don't show LeftView when empty.
                    diffViewer.ViewMode = DifferenceViewMode.RightViewOnly;
                }
                else if (rightText == string.Empty)
                {
                    // Don't show RightView when empty.
                    diffViewer.ViewMode = DifferenceViewMode.LeftViewOnly;
                }
                else if (leftText == rightText)
                {
                    // Don't show LeftView when no changes.
                    diffViewer.ViewMode = DifferenceViewMode.RightViewOnly;
                }

                AddBufferTag(diffViewer.LeftView.TextBuffer, session, leftPath, mergeBase, DiffSide.Left);

                if (!workingDirectory)
                {
                    AddBufferTag(diffViewer.RightView.TextBuffer, session, rightPath, file.CommitSha, DiffSide.Right);
                    EnableNavigateToEditor(diffViewer.LeftView, session);
                    EnableNavigateToEditor(diffViewer.RightView, session);
                    EnableNavigateToEditor(diffViewer.InlineView, session);
                }

                if (workingDirectory)
                    await usageTracker.IncrementCounter(x => x.NumberOfPRDetailsCompareWithSolution);
                else
                    await usageTracker.IncrementCounter(x => x.NumberOfPRDetailsViewChanges);

                if (openThread.line != -1)
                {
                    var view = diffViewer.ViewMode == DifferenceViewMode.Inline ?
                        diffViewer.InlineView :
                        openThread.side == DiffSide.Left ? diffViewer.LeftView : diffViewer.RightView;
                    
                    // HACK: We need to wait here for the view to initialize or the peek session won't appear.
                    // There must be a better way of doing this.
                    await Task.Delay(1500).ConfigureAwait(true);
                    peekService.Show(view, openThread.side, openThread.line);
                }

                return diffViewer;
            }
            catch (Exception e)
            {
                ShowErrorInStatusBar("Error opening file", e);
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<IDifferenceViewer> OpenDiff(
            IPullRequestSession session,
            string relativePath,
            IInlineCommentThreadModel thread)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotEmptyString(relativePath, nameof(relativePath));
            Guard.ArgumentNotNull(thread, nameof(thread));

            return OpenDiff(session, relativePath, thread.CommitSha, thread.LineNumber - 1);
        }

        /// <inheritdoc/>
        public async Task<IDifferenceViewer> OpenDiff(IPullRequestSession session, string relativePath, string headSha, int nextInlineTagFromLine)
        {
            var diffViewer = await OpenDiff(session, relativePath, headSha, scrollToFirstDraftOrDiff: false);

            var param = (object) new InlineCommentNavigationParams
            {
                FromLine = nextInlineTagFromLine,
            };

            // HACK: We need to wait here for the inline comment tags to initialize so we can find the next inline comment.
            // There must be a better way of doing this.
            await Task.Delay(1500);
            RaiseWhenAvailable(Guids.CommandSetString, PkgCmdIDList.NextInlineCommentId, param);

            return diffViewer;
        }

        static bool RaiseWhenAvailable(string guid, int id, object param)
        {
            var commands = VisualStudio.Services.Dte.Commands;
            var command = commands.Item(guid, id);

            if (command.IsAvailable)
            {
                commands.Raise(command.Guid, command.ID, ref param, null);
                return true;
            }

            return false;
        }

        public void OpenActiveDocumentInCodeView(IVsTextView sourceView)
        {
            var dte = serviceProvider.GetService<DTE>();
            // Not sure how to get a file name directly from IVsTextView. Using DTE.ActiveDocument.FullName.
            var fullPath = dte.ActiveDocument.FullName;
            // VsShellUtilities.OpenDocument with VSConstants.LOGVIEWID.Code_guid always open a new Code view.
            // Using DTE.ItemOperations.OpenFile with Constants.vsViewKindCode instead,
            dte.ItemOperations.OpenFile(fullPath, EnvDTE.Constants.vsViewKindCode);
            var codeView = FindActiveView();
            NavigateToEquivalentPosition(sourceView, codeView);
        }

        public bool IsEditableDiff(ITextView textView)
        {
            var readOnly = textView.Options.GetOptionValue(DefaultTextViewOptions.ViewProhibitUserInputId);
            var isDiff = IsDiff(textView);
            return !readOnly && isDiff;
        }

        public void NavigateToEquivalentPosition(IVsTextView sourceView, IVsTextView targetView)
        {
            int line;
            int column;
            ErrorHandler.ThrowOnFailure(sourceView.GetCaretPos(out line, out column));
            var text1 = GetText(sourceView);
            var text2 = GetText(targetView);

            var fromLines = ReadLines(text1);
            var toLines = ReadLines(text2);
            var matchingLine = FindMatchingLine(fromLines, toLines, line, matchLinesAbove: MatchLinesAboveTarget);
            if (matchingLine == -1)
            {
                // If we can't match line use orignal as best guess.
                matchingLine = line < toLines.Count ? line : toLines.Count - 1;
                column = 0;
            }

            ErrorHandler.ThrowOnFailure(targetView.SetCaretPos(matchingLine, column));
            ErrorHandler.ThrowOnFailure(targetView.CenterLines(matchingLine, 1));
        }

        public IVsTextView FindActiveView()
        {
            var textManager = serviceProvider.GetService<SVsTextManager, IVsTextManager2>();
            IVsTextView view;
            var hresult = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);
            return hresult == VSConstants.S_OK ? view : null;
        }

        /// <summary>
        /// Find the closest matching line in <see cref="toLines"/>.
        /// </summary>
        /// <remarks>
        /// When matching we prioritize unique matching lines in <see cref="toLines"/>. If the target line isn't
        /// unique, continue searching the lines above for a better match and use this as anchor with an offset.
        /// The closest match to <see cref="line"/> with the fewest duplicate matches will be used for the matching line.
        /// </remarks>
        /// <param name="fromLines">The document we're navigating from.</param>
        /// <param name="toLines">The document we're navigating to.</param>
        /// <param name="line">The 0-based line we're navigating from.</param>
        /// <param name="matchLinesAbove"></param>
        /// <returns>The best matching line in <see cref="toLines"/></returns>
        public int FindMatchingLine(IList<string> fromLines, IList<string> toLines, int line, int matchLinesAbove = 0)
        {
            var matchingLine = -1;
            var minMatchedLines = -1;
            for (var offset = 0; offset <= matchLinesAbove; offset++)
            {
                var targetLine = line - offset;
                if (targetLine < 0)
                {
                    break;
                }

                int matchedLines;
                var nearestLine = FindNearestMatchingLine(fromLines, toLines, targetLine, out matchedLines);
                if (nearestLine != -1)
                {
                    if (matchingLine == -1 || minMatchedLines >= matchedLines)
                    {
                        matchingLine = nearestLine + offset;
                        minMatchedLines = matchedLines;
                    }

                    if (minMatchedLines == 1)
                    {
                        break; // We've found a unique matching line!
                    }
                }
            }

            if (matchingLine >= toLines.Count)
            {
                matchingLine = toLines.Count - 1;
            }

            return matchingLine;
        }

        /// <summary>
        /// Find the nearest matching line to <see cref="line"/> and the number of similar matched lines in the text.
        /// </summary>
        /// <param name="fromLines">The document we're navigating from.</param>
        /// <param name="toLines">The document we're navigating to.</param>
        /// <param name="line">The 0-based line we're navigating from.</param>
        /// <param name="matchedLines">The number of similar matched lines in <see cref="toLines"/></param>
        /// <returns>Find the nearest matching line in <see cref="toLines"/>.</returns>
        public int FindNearestMatchingLine(IList<string> fromLines, IList<string> toLines, int line, out int matchedLines)
        {
            line = line < fromLines.Count ? line : fromLines.Count - 1; // VS shows one extra line at end
            var fromLine = fromLines[line];

            matchedLines = 0;
            var matchingLine = -1;
            for (var offset = 0; true; offset++)
            {
                var lineAbove = line + offset;
                var checkAbove = lineAbove < toLines.Count;
                if (checkAbove && toLines[lineAbove] == fromLine)
                {
                    if (matchedLines == 0)
                    {
                        matchingLine = lineAbove;
                    }

                    matchedLines++;
                }

                var lineBelow = line - offset;
                var checkBelow = lineBelow >= 0;
                if (checkBelow && offset > 0 && lineBelow < toLines.Count && toLines[lineBelow] == fromLine)
                {
                    if (matchedLines == 0)
                    {
                        matchingLine = lineBelow;
                    }

                    matchedLines++;
                }

                if (!checkAbove && !checkBelow)
                {
                    break;
                }
            }

            return matchingLine;
        }

        static string GetAbsolutePath(LocalRepositoryModel localRepository, string relativePath)
        {
            var localPath = localRepository.LocalPath;
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(localPath, relativePath);
        }

        string GetText(IVsTextView textView)
        {
            IVsTextLines buffer;
            ErrorHandler.ThrowOnFailure(textView.GetBuffer(out buffer));

            int line;
            int index;
            ErrorHandler.ThrowOnFailure(buffer.GetLastLineIndex(out line, out index));

            string text;
            ErrorHandler.ThrowOnFailure(buffer.GetLineText(0, 0, line, index, out text));
            return text;
        }

        IVsTextView OpenDocument(string fullPath, bool readOnly, out IWpfTextView wpfTextView)
        {
            var logicalView = VSConstants.LOGVIEWID.TextView_guid;
            IVsUIHierarchy hierarchy;
            uint itemID;
            IVsWindowFrame windowFrame;
            IVsTextView view;
            VsShellUtilities.OpenDocument(serviceProvider, fullPath, logicalView, out hierarchy, out itemID, out windowFrame, out view);

            wpfTextView = vsEditorAdaptersFactory.GetWpfTextView(view);
            wpfTextView?.Options?.SetOptionValue(DefaultTextViewOptions.ViewProhibitUserInputId, readOnly);

            return view;
        }

        IDifferenceViewer FocusExistingDiffViewer(
            IPullRequestSession session,
            string mergeBase,
            string rightPath)
        {
            IVsUIHierarchy uiHierarchy;
            uint itemID;
            IVsWindowFrame windowFrame;

            // Diff documents are indexed by the path on the right hand side of the comparison.
            if (VsShellUtilities.IsDocumentOpen(
                    serviceProvider,
                    rightPath,
                    Guid.Empty,
                    out uiHierarchy,
                    out itemID,
                    out windowFrame))
            {
                var diffViewer = GetDiffViewer(windowFrame);

                if (diffViewer != null)
                {
                    PullRequestTextBufferInfo leftBufferInfo;

                    if (diffViewer.LeftView.TextBuffer.Properties.TryGetProperty(
                            typeof(PullRequestTextBufferInfo),
                            out leftBufferInfo) &&
                        leftBufferInfo.Session.PullRequest.Number == session.PullRequest.Number &&
                        leftBufferInfo.CommitSha == mergeBase)
                    {
                        ErrorHandler.ThrowOnFailure(windowFrame.Show());
                        return diffViewer;
                    }
                }
            }

            return null;
        }

        void ShowErrorInStatusBar(string message, Exception e)
        {
            statusBar.ShowMessage(message + ": " + e.Message);
        }

        void AddBufferTag(
            ITextBuffer buffer,
            IPullRequestSession session,
            string path,
            string commitSha,
            DiffSide? side)
        {
            buffer.Properties.GetOrCreateSingletonProperty(
                typeof(PullRequestTextBufferInfo),
                () => new PullRequestTextBufferInfo(session, path, commitSha, side));

            var projection = buffer as IProjectionBuffer;

            if (projection != null)
            {
                foreach (var source in projection.SourceBuffers)
                {
                    AddBufferTag(source, session, path, commitSha, side);
                }
            }
        }

        void EnableNavigateToEditor(ITextView textView, IPullRequestSession session)
        {
            var vsTextView = vsEditorAdaptersFactory.GetViewAdapter(textView);
            EnableNavigateToEditor(vsTextView, session);
        }

        void EnableNavigateToEditor(IVsTextView vsTextView, IPullRequestSession session)
        {
            var commandGroup = VSConstants.CMDSETID.StandardCommandSet2K_guid;
            var commandId = (int)VSConstants.VSStd2KCmdID.RETURN;
            TextViewCommandDispatcher.AddCommandFilter(vsTextView, commandGroup, commandId, goToSolutionOrPullRequestFileCommand);

            EnableNavigateStatusBarMessage(vsTextView, session);
        }

        void EnableNavigateStatusBarMessage(IVsTextView vsTextView, IPullRequestSession session)
        {
            var textView = vsEditorAdaptersFactory.GetWpfTextView(vsTextView);

            var statusMessage = session.IsCheckedOut ?
                Resources.NavigateToEditorStatusMessage : Resources.NavigateToEditorNotCheckedOutStatusMessage;

            textView.GotAggregateFocus += (s, e) =>
                statusBar.ShowMessage(statusMessage);

            textView.LostAggregateFocus += (s, e) =>
                statusBar.ShowMessage(string.Empty);
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

        async Task<string> GetBaseFileName(IPullRequestSession session, IPullRequestSessionFile file)
        {
            using (var changes = await pullRequestService.GetTreeChanges(
                session.LocalRepository,
                session.PullRequest))
            {
                var fileChange = changes.FirstOrDefault(x => x.Path == file.RelativePath);
                return fileChange?.Status == LibGit2Sharp.ChangeKind.Renamed ?
                    fileChange.OldPath : file.RelativePath;
            }
        }

        static bool IsDiff(ITextView textView) => textView.Roles.Contains("DIFF");

        static IDifferenceViewer GetDiffViewer(IVsWindowFrame frame)
        {
            object docView;

            if (ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView)))
            {
                return (docView as IVsDifferenceCodeWindow)?.DifferenceViewer;
            }

            return null;
        }

        static IDisposable OpenInProvisionalTab()
        {
            return new NewDocumentStateScope(
                __VSNEWDOCUMENTSTATE.NDS_Provisional,
                VSConstants.NewDocumentStateReason.SolutionExplorer);
        }

        IDisposable OpenWithOption(string optionId, object value) => new OpenWithOptionScope(editorOptionsFactoryService, optionId, value);

        class OpenWithOptionScope : IDisposable
        {
            readonly IEditorOptionsFactoryService editorOptionsFactoryService;
            readonly string optionId;
            readonly object savedValue;

            internal OpenWithOptionScope(IEditorOptionsFactoryService editorOptionsFactoryService, string optionId, object value)
            {
                this.editorOptionsFactoryService = editorOptionsFactoryService;
                this.optionId = optionId;
                savedValue = editorOptionsFactoryService.GlobalOptions.GetOptionValue(optionId);
                editorOptionsFactoryService.GlobalOptions.SetOptionValue(optionId, value);
            }

            public void Dispose()
            {
                editorOptionsFactoryService.GlobalOptions.SetOptionValue(optionId, savedValue);
            }
        }

        static IList<string> ReadLines(string text)
        {
            var lines = new List<string>();
            var reader = new DiffUtilities.LineReader(text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines;
        }
    }
}
