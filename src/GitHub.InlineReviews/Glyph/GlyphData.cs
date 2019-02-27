using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;

namespace GitHub.InlineReviews.Glyph.Implementation
{
    /// <summary>
    /// Information about the position of a glyph.
    /// </summary>
    /// <typeparam name="TGlyphTag">The type of glyph tag we're dealing with.</typeparam>
    internal class GlyphData<TGlyphTag>
    {
        double deltaY;

        public GlyphData(SnapshotSpan visualSpan, TGlyphTag tag, UIElement element)
        {
            VisualSpan = visualSpan;
            GlyphType = tag.GetType();
            Glyph = element;

            deltaY = Canvas.GetTop(element);
            if (double.IsNaN(deltaY))
            {
                deltaY = 0.0;
            }
        }

        public void SetSnapshot(ITextSnapshot snapshot)
        {
            VisualSpan = VisualSpan.Value.TranslateTo(snapshot, SpanTrackingMode.EdgeInclusive);
        }

        public void SetTop(double top)
        {
            Canvas.SetTop(Glyph, top + deltaY);
        }

        public UIElement Glyph { get; }

        public Type GlyphType { get; }

        public SnapshotSpan? VisualSpan { get; private set; }
    }
}

