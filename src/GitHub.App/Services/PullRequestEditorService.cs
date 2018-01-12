using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Commands;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
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
        readonly IGitHubServiceProvider serviceProvider;
        readonly IPullRequestService pullRequestService;
        readonly IVsEditorAdaptersFactoryService vsEditorAdaptersFactory;
        readonly IStatusBarNotificationService statusBar;
        readonly IUsageTracker usageTracker;

        [ImportingConstructor]
        public PullRequestEditorService(
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
        }

        /// <inheritdoc/>
        public async Task OpenFile(
            IPullRequestSession session,
            string relativePath,
            bool workingDirectory)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotEmptyString(relativePath, nameof(relativePath));

            try
            {
                var file = await session.GetFile(relativePath);
                var fullPath = GetAbsolutePath(session, file);
                var fileName = workingDirectory ? fullPath : await ExtractFile(session, file, true);

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

        /// <inheritdoc/>
        public async Task OpenDiff(
            IPullRequestSession session,
            string relativePath,
            bool workingDirectory)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotEmptyString(relativePath, nameof(relativePath));

            try
            {
                var file = await session.GetFile(relativePath);
                var rightPath = file.RelativePath;
                var leftPath = await GetBaseFileName(session, file);
                var rightFile = workingDirectory ? GetAbsolutePath(session, file) : await ExtractFile(session, file, true);
                var leftFile = await ExtractFile(session, file, false);
                var leftLabel = $"{leftPath};{session.GetBaseBranchDisplay()}";
                var rightLabel = workingDirectory ? rightPath : $"{rightPath};PR {session.PullRequest.Number}";
                var caption = $"Diff - {Path.GetFileName(file.RelativePath)}";
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

        /// <inheritdoc/>
        public async Task OpenDiff(
            IPullRequestSession session,
            string relativePath,
            IInlineCommentThreadModel thread)
        {
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotEmptyString(relativePath, nameof(relativePath));
            Guard.ArgumentNotNull(thread, nameof(thread));

            await OpenDiff(session, relativePath, false);

            // HACK: We need to wait here for the diff view to set itself up and move its cursor
            // to the first changed line. There must be a better way of doing this.
            await Task.Delay(1500);

            var param = (object)new InlineCommentNavigationParams
            {
                FromLine = thread.LineNumber - 1,
            };

            VisualStudio.Services.Dte.Commands.Raise(
                Guids.CommandSetString,
                PkgCmdIDList.NextInlineCommentId,
                ref param,
                null);
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

        async Task<string> ExtractFile(IPullRequestSession session, IPullRequestSessionFile file, bool head)
        {
            var encoding = pullRequestService.GetEncoding(session.LocalRepository, file.RelativePath);
            var relativePath = head ? file.RelativePath : await GetBaseFileName(session, file);

            return await pullRequestService.ExtractFile(
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

        static string GetAbsolutePath(IPullRequestSession session, IPullRequestSessionFile file)
        {
            return Path.Combine(session.LocalRepository.LocalPath, file.RelativePath);
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
