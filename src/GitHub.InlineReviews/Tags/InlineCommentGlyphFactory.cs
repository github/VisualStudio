using System;
using System.Windows;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentGlyphFactory : IGlyphFactory<InlineCommentTag>
    {
        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineCommentTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineCommentTag;

            if (addTag != null)
            {
                return new AddInlineCommentGlyph();
            }
            else if (showTag != null)
            {
                return new ShowInlineCommentGlyph()
                {
                    Opacity = showTag.Thread.IsStale ? 0.5 : 1,
                };
            }

            return null;
        }
    }
}
