using System;
using System.Windows;
using System.Windows.Media;
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
        readonly IInlineCommentPeekService peekService;
        readonly ITextView textView;
        readonly BrushesManager brushesManager;

        public InlineCommentGlyphFactory(
            IInlineCommentPeekService peekService,
            ITextView textView,
            IEditorFormatMap editorFormatMap)
        {
            this.peekService = peekService;
            this.textView = textView;

            brushesManager = new BrushesManager(editorFormatMap);
        }

        class BrushesManager
        {
            const string AddPropertiesKey = "deltadiff.add.word";
            const string DeletePropertiesKey = "deltadiff.remove.word";
            const string NonePropertiesKey = "Indicator Margin";

            readonly ResourceDictionary addProperties;
            readonly ResourceDictionary deleteProperties;
            readonly ResourceDictionary noneProperties;

            internal BrushesManager(IEditorFormatMap editorFormatMap)
            {
                addProperties = editorFormatMap.GetProperties(AddPropertiesKey);
                deleteProperties = editorFormatMap.GetProperties(DeletePropertiesKey);
                noneProperties = editorFormatMap.GetProperties(NonePropertiesKey);
            }

            internal Brush GetBackground(DiffChangeType diffChangeType)
            {
                switch (diffChangeType)
                {
                    case DiffChangeType.Add:
                        return GetBackground(addProperties);
                    case DiffChangeType.Delete:
                        return GetBackground(deleteProperties);
                    case DiffChangeType.None:
                    default:
                        return GetBackground(noneProperties);
                }
            }

            static Brush GetBackground(ResourceDictionary dictionary)
            {
                return dictionary["Background"] as Brush;
            }
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineCommentTag tag)
        {
            var glyph = CreateGlyph(tag);
            glyph.DataContext = tag;

            glyph.MouseLeftButtonUp += (s, e) =>
            {
                if (OpenThreadView(tag)) e.Handled = true;
            };

            glyph.Resources["DiffChangeBackground"] = brushesManager.GetBackground(tag.DiffChangeType);
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
