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
    public class InlineCommentMargin : IWpfTextViewMargin
    {
        public const string MarginName = "InlineComment";
        const string MarginPropertiesName = "Indicator Margin"; // Same background color as Glyph margin 

        readonly IWpfTextViewHost wpfTextViewHost;
        readonly IInlineCommentPeekService peekService;
        readonly IEditorFormatMapService editorFormatMapService;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IPackageSettings packageSettings;
        readonly Lazy<IPullRequestSessionManager> sessionManager;
        readonly Lazy<Grid> marginGrid;

        GlyphMargin<InlineCommentTag> glyphMargin;

        public InlineCommentMargin(
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

            marginGrid = new Lazy<Grid>(() => CreateMarginGrid());
        }

        public ITextViewMargin GetTextViewMargin(string name)
        {
            return (name == MarginName) ? this : null;
        }

        public void Dispose() => glyphMargin?.Dispose();

        public FrameworkElement VisualElement => marginGrid.Value;

        public double MarginSize => marginGrid.Value.Width;

        public bool Enabled => IsMarginVisible();

        Grid CreateMarginGrid()
        {
            var marginGrid = new GlyphMarginGrid();

            var textView = wpfTextViewHost.TextView;
            var glyphFactory = new InlineCommentGlyphFactory(peekService, textView);
            var editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);

            glyphMargin = new GlyphMargin<InlineCommentTag>(wpfTextViewHost, glyphFactory, marginGrid, tagAggregatorFactory,
                editorFormatMap, IsMarginVisible, MarginPropertiesName, true, 17.0);

            if (IsDiffView(wpfTextViewHost))
            {
                TrackCommentGlyph(wpfTextViewHost, marginGrid);
            }

            return marginGrid;
        }

        public bool IsMarginDisabled(IWpfTextViewHost textViewHost) => !packageSettings.EditorComments && !IsDiffView(textViewHost);

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

        bool IsMarginVisible()
        {
            var textView = wpfTextViewHost.TextView;
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
