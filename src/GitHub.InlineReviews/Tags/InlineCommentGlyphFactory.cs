using System;
using System.Windows;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentGlyphFactory : IGlyphFactory
    {
        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            var reviewTag = (InlineCommentTag)tag;
            return new InlineCommentGlyph()
            {
                Opacity = reviewTag.NeedsUpdate ? 0.5 : 1,
            };
        }
    }
}
