using System;
using GitHub.Commands;

namespace GitHub.Commands
{
    /// <summary>
    /// Navigates to and opens the the previous inline comment thread in the currently active text view.
    /// </summary>
    public interface IPreviousInlineCommentCommand : IVsCommand<InlineCommentNavigationParams>
    {
    }
}