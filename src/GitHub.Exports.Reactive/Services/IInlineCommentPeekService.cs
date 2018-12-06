using System;
using GitHub.Models;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.Services
{
    /// <summary>
    /// Shows inline comments in a peek view.
    /// </summary>
    public interface IInlineCommentPeekService
    {
        /// <summary>
        /// Gets the 0-based line number for a peek session tracking point.
        /// </summary>
        /// <param name="session">The peek session.</param>
        /// <param name="point">The peek session tracking point</param>
        /// <returns>
        /// A tuple containing the 0-based line number and whether the line number represents a line in the
        /// left hand side of a diff view.
        /// </returns>
        Tuple<int, bool> GetLineNumber(IPeekSession session, ITrackingPoint point);

        /// <summary>
        /// Hides the inline comment peek view for a text view.
        /// </summary>
        /// <param name="textView">The text view.</param>
        void Hide(ITextView textView);

        /// <summary>
        /// Shows the peek view for on an <see cref="ITextView"/>.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <param name="tag">The tag.</param>
        ITrackingPoint Show(ITextView textView, DiffSide side, int lineNumber);
    }
}