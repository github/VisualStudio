using System;
using System.ComponentModel.Composition;
using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace GitHub.InlineReviews.Tags
{
    /// <summary>
    /// Factory class for <see cref="InlineCommentTagger"/>s.
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(ShowInlineCommentTag))]
    [TextViewRole("LEFTDIFF")]
    [TextViewRole("RIGHTDIFF")]
    [TextViewRole("INLINEDIFF")]
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
            if (view.TextViewModel is IDifferenceTextViewModel model)
            {
                if (buffer == model.Viewer.DifferenceBuffer.BaseLeftBuffer)
                {
                    return view.Properties.GetOrCreateSingletonProperty("InlineTaggerForLeftBuffer",
                        () => new InlineCommentTagger(view, buffer, sessionManager) as ITagger<T>);
                }

                if (buffer == model.Viewer.DifferenceBuffer.BaseRightBuffer)
                {
                    return view.Properties.GetOrCreateSingletonProperty("InlineTaggerForRightBuffer",
                        () => new InlineCommentTagger(view, buffer, sessionManager) as ITagger<T>);
                }
            }

            return null;
        }
    }
}
