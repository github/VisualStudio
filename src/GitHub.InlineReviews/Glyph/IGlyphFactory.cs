using System;
using System.Windows;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface IGlyphFactory<TGlyphTag> where TGlyphTag : ITag
    {
        UIElement GenerateGlyph(IWpfTextViewLine line, TGlyphTag tag);
    }
}
