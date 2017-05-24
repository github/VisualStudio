using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor.Implementation;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Microsoft.VisualStudio.Text.Editor
{
    public sealed class GlyphMargin<TGlyphTag> : IWpfTextViewMargin, ITextViewMargin, IDisposable where TGlyphTag: ITag
    {
        bool handleZoom;
        bool isDisposed;
        FrameworkElement marginVisual;
        bool refreshAllGlyphs;
        ITagAggregator<TGlyphTag> tagAggregator;
        IWpfTextView textView;
        string marginName;
        GlyphMarginVisualManager<TGlyphTag> visualManager;

        public GlyphMargin(
            IWpfTextViewHost wpfTextViewHost,
            IGlyphFactory<TGlyphTag> glyphFactory,
            ITagAggregator<TGlyphTag> tagAggregator,
            IEditorFormatMap editorFormatMap,
            string marginPropertiesName, string marginName, bool handleZoom = true, double marginWidth = 17.0)
        {
            textView = wpfTextViewHost.TextView;
            this.tagAggregator = tagAggregator;
            this.marginName = marginName;
            this.handleZoom = handleZoom;

            visualManager = new GlyphMarginVisualManager<TGlyphTag>(textView, glyphFactory,
                this, editorFormatMap, marginPropertiesName, marginWidth);

            marginVisual = visualManager.MarginVisual;
            marginVisual.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.OnIsVisibleChanged);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                marginVisual.IsVisibleChanged -= new DependencyPropertyChangedEventHandler(this.OnIsVisibleChanged);
                tagAggregator.Dispose();
                marginVisual = null;
                isDisposed = true;
            }
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            if (string.Compare(marginName, this.marginName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }

            return this;
        }

        private void OnBatchedTagsChanged(object sender, BatchedTagsChangedEventArgs e)
        {
            if (!textView.IsClosed)
            {
                List<SnapshotSpan> list = new List<SnapshotSpan>();
                foreach (IMappingSpan span in e.Spans)
                {
                    list.AddRange(span.GetSpans(textView.TextSnapshot));
                }

                if (list.Count > 0)
                {
                    SnapshotSpan span3 = list[0];
                    int start = (int) span3.Start;
                    span3 = list[0];
                    int end = (int) span3.End;
                    for (int i = 1; i < list.Count; i++)
                    {
                        span3 = list[i];
                        start = Math.Min(start, (int) span3.Start);
                        span3 = list[i];
                        end = Math.Max(end, (int) span3.End);
                    }

                    SnapshotSpan span2 = new SnapshotSpan(textView.TextSnapshot, start, end - start);
                    visualManager.RemoveGlyphsByVisualSpan(span2);
                    foreach (ITextViewLine line in textView.TextViewLines.GetTextViewLinesIntersectingSpan(span2))
                    {
                        this.RefreshGlyphsOver(line);
                    }
                }
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                if (!textView.IsClosed)
                {
                    tagAggregator.BatchedTagsChanged += new EventHandler<BatchedTagsChangedEventArgs>(OnBatchedTagsChanged);
                    textView.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(OnLayoutChanged);
                    if (handleZoom)
                    {
                        textView.ZoomLevelChanged += new EventHandler<ZoomLevelChangedEventArgs>(this.OnZoomLevelChanged);
                    }
                    if (textView.InLayout)
                    {
                        refreshAllGlyphs = true;
                    }
                    else
                    {
                        foreach (ITextViewLine line in textView.TextViewLines)
                        {
                            RefreshGlyphsOver(line);
                        }
                    }
                    if (handleZoom)
                    {
                        marginVisual.LayoutTransform = new ScaleTransform(textView.ZoomLevel / 100.0, textView.ZoomLevel / 100.0);
                        marginVisual.LayoutTransform.Freeze();
                    }
                }
            }
            else
            {
                visualManager.RemoveGlyphsByVisualSpan(new SnapshotSpan(textView.TextSnapshot, 0, textView.TextSnapshot.Length));
                tagAggregator.BatchedTagsChanged -= new EventHandler<BatchedTagsChangedEventArgs>(OnBatchedTagsChanged);
                textView.LayoutChanged -= new EventHandler<TextViewLayoutChangedEventArgs>(OnLayoutChanged);
                if (handleZoom)
                {
                    textView.ZoomLevelChanged -= new EventHandler<ZoomLevelChangedEventArgs>(OnZoomLevelChanged);
                }
            }
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            visualManager.SetSnapshotAndUpdate(textView.TextSnapshot, e.NewOrReformattedLines, e.VerticalTranslation ? (IList<ITextViewLine>)textView.TextViewLines : e.TranslatedLines);

            var lines = refreshAllGlyphs ? (IList<ITextViewLine>)textView.TextViewLines : e.NewOrReformattedLines;
            foreach (ITextViewLine line in lines)
            {
                visualManager.RemoveGlyphsByVisualSpan(line.Extent);
                RefreshGlyphsOver(line);
            }

            refreshAllGlyphs = false;
        }

        private void OnZoomLevelChanged(object sender, ZoomLevelChangedEventArgs e)
        {
            refreshAllGlyphs = true;
            marginVisual.LayoutTransform = e.ZoomTransform;
        }

        private void RefreshGlyphsOver(ITextViewLine textViewLine)
        {
            foreach (IMappingTagSpan<TGlyphTag> span in tagAggregator.GetTags(textViewLine.ExtentAsMappingSpan))
            {
                NormalizedSnapshotSpanCollection spans;
                if (span.Span.Start.GetPoint(textView.VisualSnapshot.TextBuffer, PositionAffinity.Predecessor).HasValue && ((spans = span.Span.GetSpans(textView.TextSnapshot)).Count > 0))
                {
                    visualManager.AddGlyph(span.Tag, spans[0]);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(marginName);
            }
        }

        public bool Enabled
        {
            get
            {
                ThrowIfDisposed();
                return textView.Options.IsGlyphMarginEnabled();
            }
        }

        public double MarginSize
        {
            get
            {
                ThrowIfDisposed();
                return marginVisual.Width;
            }
        }

        public FrameworkElement VisualElement
        {
            get
            {
                ThrowIfDisposed();
                return marginVisual;
            }
        }
    }
}
