using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.VisualStudio.Text.Editor.Implementation
{
    internal class CanvasAndGlyphFactory<TGlyphTag> where TGlyphTag: ITag
    {
        public CanvasAndGlyphFactory(Canvas glyphCanvas)
        {
            GlyphCanvas = glyphCanvas;
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, TGlyphTag tag)
        {
            return GlyphFactory?.GenerateGlyph(line, tag);
        }

        public bool HasFactory => GlyphFactory != null;

        public IGlyphFactory<TGlyphTag> GlyphFactory { private get; set; }

        public Canvas GlyphCanvas { get; }
    }
}

