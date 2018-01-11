using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;
using GitHub.Models;

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

            var matchingLine = FindNearestMatchingLine(text1, text2, line);
            if (matchingLine == -1)
            {
                // If we can't match line use orignal as best guess.
                matchingLine = line;
                column = 0;
            }

            ErrorHandler.ThrowOnFailure(view.SetCaretPos(matchingLine, column));
            ErrorHandler.ThrowOnFailure(view.CenterLines(matchingLine, 1));

            return view;
        }

        public IVsTextView FindActiveView()
        {
            var textManager = (IVsTextManager2)serviceProvider.GetService(typeof(SVsTextManager));
            IVsTextView view;
            var hresult = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out view);
            return hresult == VSConstants.S_OK ? view : null;
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

        static int FindNearestMatchingLine(string text1, string text2, int line)
        {
            var fromLines = ReadLines(text1);
            var toLines = ReadLines(text2);
            var fromLine = fromLines[line];

            for (var offset = 0; true; offset++)
            {
                var lineAbove = line + offset;
                var checkAbove = lineAbove < toLines.Count;
                if (checkAbove && toLines[lineAbove] == fromLine)
                {
                    return lineAbove;
                }

                var lineBelow = line - offset;
                var checkBelow = lineBelow >= 0;
                if (checkBelow && toLines[lineBelow] == fromLine)
                {
                    return lineBelow;
                }

                if (!checkAbove && !checkBelow)
                {
                    return -1;
                }
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
