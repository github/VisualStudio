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
            const string AddBackgroundColorKey = "deltadiff.add.word";
            const string DeleteBackgroundColorKey = "deltadiff.remove.word";
            const string NoneBackgroundColorKey = "Indicator Margin";

            readonly IEditorFormatMap editorFormatMap;
            readonly SolidColorBrush addBackground;
            readonly SolidColorBrush deleteBackground;
            readonly SolidColorBrush noneBackground;

            internal BrushesManager(IEditorFormatMap editorFormatMap)
            {
                this.editorFormatMap = editorFormatMap;

                addBackground = new SolidColorBrush();
                deleteBackground = new SolidColorBrush();
                noneBackground = new SolidColorBrush();
                UpdateBrushColors();

                editorFormatMap.FormatMappingChanged += EditorFormatMap_FormatMappingChanged;
            }

            internal Brush GetBrush(DiffChangeType diffChangeType)
            {
                switch (diffChangeType)
                {
                    case DiffChangeType.Add:
                        return addBackground;
                    case DiffChangeType.Delete:
                        return deleteBackground;
                    case DiffChangeType.None:
                    default:
                        return noneBackground;
                }
            }

            void EditorFormatMap_FormatMappingChanged(object sender, FormatItemsEventArgs e)
            {
                foreach (var key in e.ChangedItems)
                {
                    UpdateBrushColors(key);
                }
            }

            void UpdateBrushColors(string key = null)
            {
                if (key == null || key == AddBackgroundColorKey)
                {
                    addBackground.Color = TryGetBackgroundColor(AddBackgroundColorKey);
                }

                if (key == null || key == DeleteBackgroundColorKey)
                {
                    deleteBackground.Color = TryGetBackgroundColor(DeleteBackgroundColorKey);
                }

                if (key == null || key == "Indicator Margin")
                {
                    noneBackground.Color = TryGetBackgroundColor(NoneBackgroundColorKey);
                }
            }

            Color TryGetBackgroundColor(string key)
            {
                try
                {
                    var properties = editorFormatMap.GetProperties(key);
                    return (Color)properties?["BackgroundColor"];
                }
                catch
                {
                    return Colors.Transparent;
                }
            }
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, InlineCommentTag tag)
        {
            var glyph = CreateGlyph(tag);
            glyph.MouseLeftButtonUp += (s, e) =>
            {
                if (OpenThreadView(tag)) e.Handled = true;
            };

            glyph.Background = brushesManager.GetBrush(tag.DiffChangeType);
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
