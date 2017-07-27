using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.Glyph;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.Views;
using GitHub.Services;
using GitHub.Settings;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(MarginName)]
    [Order(After = PredefinedMarginNames.Glyph)]
    [MarginContainer(PredefinedMarginNames.Left)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class InlineCommentMarginProvider : IWpfTextViewMarginProvider
    {
        const string MarginName = "InlineComment";
        const string MarginPropertiesName = "Indicator Margin"; // Same background color as Glyph margin 

        readonly IEditorFormatMapService editorFormatMapService;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IInlineCommentPeekService peekService;
        readonly IPullRequestSessionManager sessionManager;
        readonly IPackageSettings packageSettings;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService,
            IPullRequestSessionManager sessionManager,
            IPackageSettings packageSettings)
        {
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = peekService;
            this.sessionManager = sessionManager;
            this.packageSettings = packageSettings;
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            var isDiffView = IsDiffView(wpfTextViewHost);
            var glyphMarginViewModel = new GlyphMarginViewModel(packageSettings, sessionManager, wpfTextViewHost, isDiffView);
            var glyphMarginView = new GlyphMarginView { DataContext = glyphMarginViewModel };

            var textView = wpfTextViewHost.TextView;
            var editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);
            var glyphFactory = new InlineCommentGlyphFactory(peekService, textView, editorFormatMap);
            return CreateMargin(glyphFactory, glyphMarginView, wpfTextViewHost, parent, editorFormatMap);
        }

        IWpfTextViewMargin CreateMargin<TGlyphTag>(IGlyphFactory<TGlyphTag> glyphFactory, Grid marginVisual,
            IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent, IEditorFormatMap editorFormatMap) where TGlyphTag : ITag
        {
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<TGlyphTag>(wpfTextViewHost.TextView);
            var margin = new GlyphMargin<TGlyphTag>(wpfTextViewHost, glyphFactory, marginVisual, tagAggregator, editorFormatMap,
                MarginPropertiesName, MarginName, true, 17.0);

            if(IsDiffView(wpfTextViewHost))
            {
                TrackCommentGlyph(wpfTextViewHost, marginVisual);
            }

            return margin;
        }

        static bool IsDiffView(IWpfTextViewHost host) => host.TextView.Roles.Contains("DIFF");

        void TrackCommentGlyph(IWpfTextViewHost host, UIElement marginElement)
        {
            var router = new MouseEnterAndLeaveEventRouter<AddInlineCommentGlyph>();
            router.Add(host.HostControl, marginElement);
        }
    }
}
