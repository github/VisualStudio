using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Text.Editor
{
    public interface IGlyphFactory<TGlyphTag> where TGlyphTag : ITag
    {
        UIElement GenerateGlyph(IWpfTextViewLine line, TGlyphTag tag);

        IEnumerable<Type> GetTagTypes();
    }
}
