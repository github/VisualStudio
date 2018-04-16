using System;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using GitHub.InlineReviews.Glyph.Implementation;
using ReactiveUI;

namespace GitHub.InlineReviews.Glyph
{
    /// <summary>
    /// Responsibe for updating the margin when tags change.
    /// </summary>
    /// <typeparam name="TGlyphTag">The type of glyph tag we're managing.</typeparam>
    public sealed class GlyphMargin<TGlyphTag> : IDisposable where TGlyphTag : ITag
    {
        readonly IWpfTextView textView;
        readonly Grid marginGrid;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly GlyphMarginVisualManager<TGlyphTag> visualManager;

        bool initialized;
        bool refreshAllGlyphs;
        ITagAggregator<TGlyphTag> tagAggregator;

        public GlyphMargin(
            IWpfTextView textView,
            IGlyphFactory<TGlyphTag> glyphFactory,
            Grid marginGrid,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IEditorFormatMap editorFormatMap,
            string marginPropertiesName)
        {
            this.textView = textView;
            this.marginGrid = marginGrid;
            this.tagAggregatorFactory = tagAggregatorFactory;
            visualManager = new GlyphMarginVisualManager<TGlyphTag>(textView, glyphFactory, marginGrid, editorFormatMap, marginPropertiesName);

            marginGrid.WhenAnyValue(x => x.IsVisible).Subscribe(IsVisibleChanged);
        }

        public void Dispose()
        {
            TryUninitialize();
        }

        void IsVisibleChanged(bool isVisible)
        {
            if (isVisible)
            {
                TryInitialize();
            }
            else
            {
                TryUninitialize();
            }
        }

        void TryInitialize()
        {
            if (!initialized)
            {
                initialized = true;
                tagAggregator = tagAggregatorFactory.CreateTagAggregator<TGlyphTag>(textView);
                tagAggregator.BatchedTagsChanged += OnBatchedTagsChanged;
                textView.LayoutChanged += OnLayoutChanged;
                textView.ZoomLevelChanged += OnZoomLevelChanged;

                if (textView.InLayout)
                {
                    refreshAllGlyphs = true;
                }
                else
                {
                    foreach (var line in textView.TextViewLines)
                    {
                        RefreshGlyphsOver(line);
                    }
                }

                marginGrid.LayoutTransform = new ScaleTransform(textView.ZoomLevel / 100.0, textView.ZoomLevel / 100.0);
                marginGrid.LayoutTransform.Freeze();
            }
        }

        void TryUninitialize()
        {
            if (initialized)
            {
                initialized = false;
                tagAggregator.BatchedTagsChanged -= OnBatchedTagsChanged;
                textView.LayoutChanged -= OnLayoutChanged;
                textView.ZoomLevelChanged -= OnZoomLevelChanged;
                tagAggregator.Dispose();
                tagAggregator = null;
            }
        }

        void OnBatchedTagsChanged(object sender, BatchedTagsChangedEventArgs e)
        {
            if (!textView.IsClosed)
            {
                var list = new List<SnapshotSpan>();
                foreach (var span in e.Spans)
                {
                    list.AddRange(span.GetSpans(textView.TextSnapshot));
                }

                if (list.Count > 0)
                {
                    var span = list[0];
                    int start = span.Start;
                    int end = span.End;
                    for (int i = 1; i < list.Count; i++)
                    {
                        span = list[i];
                        start = Math.Min(start, span.Start);
                        end = Math.Max(end, span.End);
                    }

                    var rangeSpan = new SnapshotSpan(textView.TextSnapshot, start, end - start);
                    visualManager.RemoveGlyphsByVisualSpan(rangeSpan);
                    foreach (var line in textView.TextViewLines.GetTextViewLinesIntersectingSpan(rangeSpan))
                    {
                        RefreshGlyphsOver(line);
                    }
                }
            }
        }

        void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            visualManager.SetSnapshotAndUpdate(textView.TextSnapshot, e.NewOrReformattedLines, e.VerticalTranslation ? (IList<ITextViewLine>)textView.TextViewLines : e.TranslatedLines);

            var lines = refreshAllGlyphs ? (IList<ITextViewLine>)textView.TextViewLines : e.NewOrReformattedLines;
            foreach (var line in lines)
            {
                visualManager.RemoveGlyphsByVisualSpan(line.Extent);
                RefreshGlyphsOver(line);
            }

            refreshAllGlyphs = false;
        }

        void OnZoomLevelChanged(object sender, ZoomLevelChangedEventArgs e)
        {
            refreshAllGlyphs = true;
            marginGrid.LayoutTransform = e.ZoomTransform;
        }

        void RefreshGlyphsOver(ITextViewLine textViewLine)
        {
            foreach (IMappingTagSpan<TGlyphTag> span in tagAggregator.GetTags(textViewLine.ExtentAsMappingSpan))
            {
                NormalizedSnapshotSpanCollection spans;
                if (span.Span.Start.GetPoint(textView.VisualSnapshot.TextBuffer, PositionAffinity.Predecessor).HasValue &&
                    ((spans = span.Span.GetSpans(textView.TextSnapshot)).Count > 0))
                {
                    visualManager.AddGlyph(span.Tag, spans[0]);
                }
            }
        }
    }
}
