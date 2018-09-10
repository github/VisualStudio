using System;

namespace GitHub.Commands
{
    /// <summary>
    /// Supplies parameters to <see cref="INextInlineCommentCommand"/> and
    /// <see cref="IPreviousInlineCommentCommand"/>.
    /// </summary>
    public class InlineReviewNavigationParams
    {
        /// <summary>
        /// Gets or sets the line that should be used as the start point for navigation.
        /// </summary>
        /// <remarks>
        /// If null, the current line will be used. If -1 then the absolute first or last
        /// comment in the file will be navigated to.
        /// </remarks>
        public int? FromLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the cursor will be moved to the newly opened
        /// comment.
        /// </summary>
        public bool MoveCursor { get; set; } = true;
    }
}
