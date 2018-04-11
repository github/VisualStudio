using System;
using System.Windows;
using System.Windows.Controls;
using GitHub.Services;
using GitHub.Settings;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.Glyph;
using GitHub.InlineReviews.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Classification;

namespace GitHub.InlineReviews
{
    public class InlineCommentMarginFactory
    {
        public const string MarginName = "InlineComment";
        const string MarginPropertiesName = "Indicator Margin"; // Same background color as Glyph margin 

        readonly IWpfTextViewHost wpfTextViewHost;
        readonly IInlineCommentPeekService peekService;
        readonly IEditorFormatMapService editorFormatMapService;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IPackageSettings packageSettings;
        readonly Lazy<IPullRequestSessionManager> sessionManager;

        public InlineCommentMarginFactory(
            IWpfTextViewHost wpfTextViewHost,
            IInlineCommentPeekService peekService,
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IPackageSettings packageSettings,
            Lazy<IPullRequestSessionManager> sessionManager)
        {
            this.wpfTextViewHost = wpfTextViewHost;
            this.peekService = peekService;
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.packageSettings = packageSettings;
            this.sessionManager = sessionManager;
        }

        public IWpfTextViewMargin Create()
        {
            if (IsMarginDisabled(wpfTextViewHost))
            {
                return null;
            }

            var textView = wpfTextViewHost.TextView;
            var glyphFactory = new InlineCommentGlyphFactory(peekService, textView);

            Func<Grid> gridFactory = () => new GlyphMarginGrid();
            var editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);
            return CreateMargin(glyphFactory, gridFactory, wpfTextViewHost, editorFormatMap);
        }

        IWpfTextViewMargin CreateMargin<TGlyphTag>(IGlyphFactory<TGlyphTag> glyphFactory, Func<Grid> gridFactory,
            IWpfTextViewHost wpfTextViewHost, IEditorFormatMap editorFormatMap) where TGlyphTag : ITag
        {
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<TGlyphTag>(wpfTextViewHost.TextView);
            var margin = new GlyphMargin<TGlyphTag>(wpfTextViewHost, glyphFactory, gridFactory, tagAggregator, editorFormatMap,
                IsMarginVisible, MarginPropertiesName, MarginName, true, 17.0);

            if (IsDiffView(wpfTextViewHost))
            {
                TrackCommentGlyph(wpfTextViewHost, margin.VisualElement);
            }

            return margin;
        }

        bool IsMarginDisabled(IWpfTextViewHost textViewHost) => !packageSettings.EditorComments && !IsDiffView(textViewHost);

        bool IsDiffView(IWpfTextViewHost host)
        {
            var textView = host.TextView;
            return textView.Roles.Contains("DIFF");
        }

        void TrackCommentGlyph(IWpfTextViewHost host, UIElement marginElement)
        {
            var router = new MouseEnterAndLeaveEventRouter<AddInlineCommentGlyph>();
            router.Add(host.HostControl, marginElement);
        }

        bool IsMarginVisible(ITextView textView)
        {
            if (!textView.Options.GetOptionValue<bool>(InlineCommentMarginEnabled.OptionName))
            {
                return false;
            }

            return IsMarginVisible(textView.TextBuffer);
        }

        bool IsMarginVisible(ITextBuffer buffer)
        {
            if (sessionManager.Value.GetTextBufferInfo(buffer) != null)
            {
                return true;
            }

            InlineCommentTagger inlineCommentTagger;
            if (buffer.Properties.TryGetProperty(typeof(InlineCommentTagger), out inlineCommentTagger))
            {
                if (inlineCommentTagger.ShowMargin)
                {
                    return true;
                }
            }

            var projection = buffer as IProjectionBuffer;
            if (projection != null)
            {
                foreach (var source in projection.SourceBuffers)
                {
                    if (IsMarginVisible(source))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
