using System;
using System.Windows;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace GitHub.InlineReviews.Tags
{
    class ReviewGlyphFactory : IGlyphFactory
    {
        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            var reviewTag = (ReviewTag)tag;
            return new ReviewGlyph()
            {
                Opacity = reviewTag.NeedsUpdate ? 0.5 : 1,
            };
        }
    }
}
