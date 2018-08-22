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

namespace GitHub.InlineReviews.Tags
{
    class InlineGlyphFactory : IGlyphFactory<InlineTagBase>
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

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineTagBase tag)
        {
            var glyph = CreateGlyph(tag);
            glyph.DataContext = tag;
            glyph.MouseLeftButtonUp += (s, e) =>
            {
                if (tag is InlineCommentTag inlineCommentTag)
                {
                    if (OpenThreadView(inlineCommentTag)) e.Handled = true;
                }

                if (tag is ShowInlineAnnotationTag showInlineAnnotationTag)
                {
                }
            };

            return glyph;
        }

        public IEnumerable<Type> GetTagTypes()
        {
            return new[]
            {
                typeof(AddInlineCommentTag),
                typeof(ShowInlineCommentTag),
                typeof(ShowInlineAnnotationTag)
            };
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        static UserControl CreateGlyph(InlineTagBase tag)
        {
            if (tag is AddInlineCommentTag addCommentTag)
            {
                return new AddInlineCommentGlyph();
            }

            if (tag is ShowInlineCommentTag showCommentTag)
            {
                return new ShowInlineCommentGlyph()
                {
                    Opacity = showCommentTag.Thread.IsStale ? 0.5 : 1,
                };
            }

            if (tag is ShowInlineAnnotationTag showAnnotation)
            {
                switch (showAnnotation.Annotation.AnnotationLevel)
                {
                    case CheckAnnotationLevel.Failure:
                        return new ShowInlineAnnotationFailureGlyph();
                    case CheckAnnotationLevel.Notice:
                       return new ShowInlineAnnotationNoticeGlyph();
                    case CheckAnnotationLevel.Warning:
                        return new ShowInlineAnnotationWarningGlyph();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
