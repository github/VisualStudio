using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;

namespace GitHub.Services
{
    [Export(typeof(INavigationService))]
    public class NavigationService : INavigationService
    {
        readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public NavigationService([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IVsTextView NavigateToEquivalentPosition(IVsTextView sourceView, string targetFile)
        {
            int line;
            int column;
            ErrorHandler.ThrowOnFailure(sourceView.GetCaretPos(out line, out column));
            var text1 = GetText(sourceView);

            var view = OpenDocument(targetFile);
            var text2 = VsShellUtilities.GetRunningDocumentContents(serviceProvider, targetFile);

            var equivalentLine = FindEquivalentLine(text1, text2, line);

            ErrorHandler.ThrowOnFailure(view.SetCaretPos(equivalentLine, column));
            ErrorHandler.ThrowOnFailure(view.CenterLines(equivalentLine, 1));

            return view;
        }

        public IVsTextView FindActiveView()
        {
            var textManager = (IVsTextManager2)serviceProvider.GetService(typeof(SVsTextManager));
            IVsTextView view;
            var hresult = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);
            return hresult == VSConstants.S_OK ? view : null;
        }

        int FindEquivalentLine(string text1, string text2, int line)
        {
            if (text1 == text2)
            {
                return line;
            }

            // TODO: Add best guess line matching when target file has been modified.
            return line;
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

        IVsTextView OpenDocument(string fullPath)
        {
            var logicalView = VSConstants.LOGVIEWID.TextView_guid;
            IVsUIHierarchy hierarchy;
            uint itemID;
            IVsWindowFrame windowFrame;
            IVsTextView view;
            VsShellUtilities.OpenDocument(serviceProvider, fullPath, logicalView, out hierarchy, out itemID, out windowFrame, out view);
            return view;
        }
    }
}
