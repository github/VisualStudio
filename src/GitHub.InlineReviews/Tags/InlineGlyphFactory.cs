using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GitHub.InlineReviews.Glyph;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using GitHub.InlineReviews.Services;

namespace GitHub.InlineReviews.Tags
{
    class InlineGlyphFactory : IGlyphFactory<InlineReviewTag>
    {
        readonly IInlineCommentPeekService peekService;
        readonly ITextView textView;

        public InlineGlyphFactory(
            IInlineCommentPeekService peekService,
            ITextView textView)
        {
            this.peekService = peekService;
            this.textView = textView;
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineReviewTag tag)
        {
            var glyph = CreateGlyph(tag);
            glyph.DataContext = tag;
            glyph.MouseLeftButtonUp += (s, e) =>
            {
                if (OpenThreadView(tag)) e.Handled = true;
            };

            return glyph;
        }

        public IEnumerable<Type> GetTagTypes()
        {
            return new[]
            {
                typeof(AddInlineCommentTag),
                typeof(ShowInlineReviewTag)
            };
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        static UserControl CreateGlyph(InlineReviewTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineReviewTag;

            if (addTag != null)
            {
                return new AddInlineCommentGlyph();
            }

            if (showTag != null)
            {
                if (showTag.Thread != null && showTag.Annotations != null)
                {
                    return new ShowInlineCommentAnnotationGlyph();
                }

                if (showTag.Thread != null)
                {
                    return new ShowInlineCommentGlyph
                    {
                        Opacity = showTag.Thread.IsStale ? 0.5 : 1,
                    };
                }

                if (showTag.Annotations != null)
                {
                    return new ShowInlineAnnotationGlyph();
                }

                throw new ArgumentException($"{nameof(showTag)} does not have a thread or annotations");
            }

            throw new ArgumentException($"Unknown 'InlineReviewTag' type '{tag}'");
        }

        bool OpenThreadView(InlineReviewTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineReviewTag;

            if (addTag != null)
            {
                peekService.Show(textView, addTag);
                return true;
            }
            else if (showTag != null)
            {
                peekService.Show(textView, showTag);
                return true;
            }

            return false;
        }
    }
}
