using System;
using System.Windows;
using System.Collections.Generic;
using GitHub.InlineReviews.Peek;
using GitHub.InlineReviews.Glyph;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Formatting;
using GitHub.InlineReviews.Services;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentGlyphFactory : IGlyphFactory<InlineCommentTag>
    {
        readonly IInlineCommentPeekService peekService;
        readonly ITextView textView;
        readonly ITagAggregator<InlineCommentTag> tagAggregator;

        public InlineCommentGlyphFactory(
            IInlineCommentPeekService peekService,
            ITextView textView,
            ITagAggregator<InlineCommentTag> tagAggregator)
        {
            this.peekService = peekService;
            this.textView = textView;
            this.tagAggregator = tagAggregator;
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineCommentTag tag)
        {
            var glyph = CreateGlyph(tag);
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

        static UIElement CreateGlyph(InlineCommentTag tag)
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
                peekService.Show(addTag);
                return true;
            }
            else if (showTag != null)
            {
                peekService.Show(showTag);
                return true;
            }

            return false;
        }
    }
}
