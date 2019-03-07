using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GitHub.InlineReviews.Glyph;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using GitHub.InlineReviews.Services;
using GitHub.Models;
using GitHub.Services;

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

            if (showTag != null)
            {
                if (showTag.Thread != null && showTag.Annotations != null)
                {
                    return new ShowInlineCommentAnnotationGlyph();
                }

                if (showTag.Thread != null)
                {
                    return new ShowInlineCommentGlyph();
                }

                if (showTag.Annotations != null)
                {
                    return new ShowInlineAnnotationGlyph();
                }

                throw new ArgumentException($"{nameof(showTag)} does not have a thread or annotations");
            }

            throw new ArgumentException($"Unknown 'InlineCommentTag' type '{tag}'");
        }

        bool OpenThreadView(InlineCommentTag tag)
        {
            var addTag = tag as AddInlineCommentTag;
            var showTag = tag as ShowInlineCommentTag;

            if (addTag != null)
            {
                var side = addTag.DiffChangeType == DiffChangeType.Delete ? DiffSide.Left : DiffSide.Right;
                peekService.Show(textView, side, addTag.LineNumber);
                return true;
            }
            else if (showTag != null)
            {
                var side = showTag.DiffChangeType == DiffChangeType.Delete ? DiffSide.Left : DiffSide.Right;
                peekService.Show(textView, side, showTag.LineNumber);
                return true;
            }

            return false;
        }
    }
}
