using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using GitHub.InlineReviews.Glyph;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Classification;
using GitHub.InlineReviews.Services;
using GitHub.Models;

namespace GitHub.InlineReviews.Tags
{
    class InlineCommentGlyphFactory : IGlyphFactory<InlineCommentTag>
    {
        const string AddBackgroundKey = "deltadiff.add.word";
        const string DeleteBackgroundKey = "deltadiff.remove.word";
        const string NoneBackgroundKey = "Indicator Margin";

        readonly IInlineCommentPeekService peekService;
        readonly ITextView textView;

        readonly ResourceDictionary addResourceDictionary;
        readonly ResourceDictionary deleteResourceDictionary;
        readonly ResourceDictionary noneResourceDictionary;

        public InlineCommentGlyphFactory(
            IInlineCommentPeekService peekService,
            ITextView textView,
            IEditorFormatMap editorFormatMap)
        {
            this.peekService = peekService;
            this.textView = textView;

            addResourceDictionary = editorFormatMap.GetProperties(AddBackgroundKey);
            deleteResourceDictionary = editorFormatMap.GetProperties(DeleteBackgroundKey);
            noneResourceDictionary = editorFormatMap.GetProperties(NoneBackgroundKey);
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineCommentTag tag)
        {
            var glyph = CreateGlyph(tag);
            glyph.MouseLeftButtonUp += (s, e) =>
            {
                if (OpenThreadView(tag)) e.Handled = true;
            };

            var dictionary = GetResourceDictionary(tag.DiffChangeType);
            glyph.Resources.MergedDictionaries.Add(dictionary);

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

        internal ResourceDictionary GetResourceDictionary(DiffChangeType diffChangeType)
        {
            switch (diffChangeType)
            {
                case DiffChangeType.Add:
                    return addResourceDictionary;
                case DiffChangeType.Delete:
                    return deleteResourceDictionary;
                case DiffChangeType.None:
                default:
                    return noneResourceDictionary;
            }
        }

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
