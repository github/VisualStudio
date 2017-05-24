using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.Text.Editor.Implementation
{
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

