using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using GitHub.Models;

namespace GitHub.Services
{
    [Export(typeof(INavigationService))]
    public class NavigationService : INavigationService
    {
        readonly IGitHubServiceProvider serviceProvider;

        // If the target line doesn't have a unique match, search this number of lines above looking for a match.
        public const int MatchLinesAboveTarget = 4;

        [ImportingConstructor]
        public NavigationService(IGitHubServiceProvider serviceProvider)
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

            var fromLines = ReadLines(text1);
            var toLines = ReadLines(text2);
            var matchingLine = FindMatchingLine(fromLines, toLines, line);
            if (matchingLine == -1)
            {
                // If we can't match line use orignal as best guess.
                matchingLine = line < toLines.Count ? line : toLines.Count - 1;
                column = 0;
            }

            ErrorHandler.ThrowOnFailure(view.SetCaretPos(matchingLine, column));
            ErrorHandler.ThrowOnFailure(view.CenterLines(matchingLine, 1));

            return view;
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
        /// <returns>The best matching line in <see cref="toLines"/></returns>
        public int FindMatchingLine(IList<string> fromLines, IList<string> toLines, int line)
        {
            var matchingLine = -1;
            var minMatchedLines = -1;
            for (var offset = 0; offset <= MatchLinesAboveTarget; offset++)
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
