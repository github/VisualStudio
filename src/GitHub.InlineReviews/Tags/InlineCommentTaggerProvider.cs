using System;
using System.ComponentModel.Composition;
using GitHub.Extensions;
using GitHub.InlineReviews.Services;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// Factory class for <see cref="InlineTagger"/>s.
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(ShowInlineTag))]
    class InlineCommentTaggerProvider : IViewTaggerProvider
    {
        readonly IPullRequestSessionManager sessionManager;

        [ImportingConstructor]
        public InlineCommentTaggerProvider(
            IPullRequestSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.sessionManager = sessionManager;
        }

        public ITagger<T> CreateTagger<T>(ITextView view, ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() =>
                new InlineTagger(
                    view,
                    buffer,
                    sessionManager)) as ITagger<T>;
        }
    }
}
