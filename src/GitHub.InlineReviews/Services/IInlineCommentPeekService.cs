using GitHub.InlineReviews.Tags;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Shows inline comments in a peek view.
    /// </summary>
    public interface IInlineCommentPeekService
    {
        /// <summary>
        /// Gets the line number that the peek session is opened at.
        /// </summary>
        /// <param name="session">The peek session.</param>
        /// <returns>
        /// The line number or null if the line number could not be determined.
        /// </returns>
        int? GetLineNumber(IPeekSession session);

        /// <summary>
        /// Hides the inline comment peek view for a text view.
        /// </summary>
        /// <param name="textView">The text view.</param>
        void Hide(ITextView textView);

        /// <summary>
        /// Shows the peek view for a <see cref="ShowInlineCommentTag"/>.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="moveCaret">
        /// Whether the caret should be moved to the line with the comments.
        /// </param>
        void Show(ITextView textView, ShowInlineCommentTag tag, bool moveCaret = false);

        /// <summary>
        /// Shows the peek view for an <see cref="AddInlineCommentTag"/>.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="moveCaret">
        /// Whether the caret should be moved to the line with the comments.
        /// </param>
        void Show(ITextView textView, AddInlineCommentTag tag, bool moveCaret = false);
    }
}