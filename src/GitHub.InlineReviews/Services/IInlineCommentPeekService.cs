using System;
using GitHub.InlineReviews.Tags;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Shows inline comments in a peek view.
    /// </summary>
    public interface IInlineCommentPeekService
    {
        /// <summary>
        /// Gets the line number for a peek session tracking point.
        /// </summary>
        /// <param name="session">The peek session.</param>
        /// <param name="point">The peek session tracking point</param>
        /// <returns>
        /// A tuple containing the line number and whether the line number represents a line in the
        /// left hand side of a diff view.
        /// </returns>
        Tuple<int, bool> GetLineNumber(IPeekSession session, ITrackingPoint point);

        /// <summary>
        /// Hides the inline comment peek view for a text view.
        /// </summary>
        /// <param name="textView">The text view.</param>
        void Hide(ITextView textView);

        /// <summary>
        /// Shows the peek view for a <see cref="ShowInlineReviewTag"/>.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <param name="tag">The tag.</param>
        ITrackingPoint Show(ITextView textView, ShowInlineReviewTag tag);

        /// <summary>
        /// Shows the peek view for an <see cref="AddInlineCommentTag"/>.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <param name="tag">The tag.</param>
        ITrackingPoint Show(ITextView textView, AddInlineCommentTag tag);
    }
}