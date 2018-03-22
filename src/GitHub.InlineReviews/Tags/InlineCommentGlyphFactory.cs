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
    class InlineCommentGlyphFactory : IGlyphFactory<InlineCommentTag>
    {
        readonly IInlineCommentPeekService peekService;
        readonly ITextView textView;

        public InlineCommentGlyphFactory(
            IInlineCommentPeekService peekService,
            ITextView textView)
        {
            this.peekService = peekService;
            this.textView = textView;
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineCommentTag tag)
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
                typeof(ShowInlineCommentTag)
            };
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        static UserControl CreateGlyph(InlineCommentTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineCommentTag;

            if (addTag != null)
            {
                return new AddInlineCommentGlyph();
            }
            else if (showTag != null)
            {
                return new ShowInlineCommentGlyph()
                {
                    Opacity = showTag.Thread.IsStale ? 0.5 : 1,
                };
            }

            throw new ArgumentException($"Unknown 'InlineCommentTag' type '{tag}'");
        }

        bool OpenThreadView(InlineCommentTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineCommentTag;

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
