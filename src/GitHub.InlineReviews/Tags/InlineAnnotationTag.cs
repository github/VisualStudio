using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// Base class for inline annotation tags.
    /// </summary>
    public abstract class InlineAnnotationTag : InlineTagBase
    {
        protected InlineAnnotationTag(
            IPullRequestSession session,
            int lineNumber) : base(session, lineNumber)
        {
        }
    }
}