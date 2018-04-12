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

        readonly IWpfTextView textView;
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
            textView = wpfTextViewHost.TextView;
            this.peekService = peekService;
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.packageSettings = packageSettings;
            this.sessionManager = sessionManager;

            marginGrid = new Lazy<Grid>(() => CreateMarginGrid(wpfTextViewHost));

            if (IsMarginDisabledDefault())
            {
                textView.Options.SetOptionValue(InlineCommentMarginEnabled.OptionName, false);
            }
        }

        public ITextViewMargin GetTextViewMargin(string name)
        {
            return (name == MarginName) ? this : null;
        }

        public void Dispose() => glyphMargin?.Dispose();

        public FrameworkElement VisualElement => marginGrid.Value;

        public double MarginSize => marginGrid.Value.Width;

        public bool Enabled => IsMarginVisible();

        Grid CreateMarginGrid(IWpfTextViewHost wpfTextViewHost)
        {
            var marginGrid = new GlyphMarginGrid { Width = 17.0 };

            var glyphFactory = new InlineCommentGlyphFactory(peekService, textView);
            var editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);

            glyphMargin = new GlyphMargin<InlineCommentTag>(textView, glyphFactory, marginGrid, tagAggregatorFactory,
                editorFormatMap, IsMarginVisible, MarginPropertiesName);

            if (IsDiffView())
            {
                TrackCommentGlyph(wpfTextViewHost, marginGrid);
            }

            return marginGrid;
        }

        bool IsMarginDisabledDefault() => !packageSettings.EditorComments && !IsDiffView();

        bool IsDiffView() => textView.Roles.Contains("DIFF");

        void TrackCommentGlyph(IWpfTextViewHost host, UIElement marginElement)
        {
            var router = new MouseEnterAndLeaveEventRouter<AddInlineCommentGlyph>();
            router.Add(host.HostControl, marginElement);
        }

        bool IsMarginVisible()
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
