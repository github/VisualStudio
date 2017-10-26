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
using Microsoft.VisualStudio.Text.Projection;

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

        readonly IGitHubServiceProvider serviceProvider;
        readonly IEditorFormatMapService editorFormatMapService;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        Lazy<IInlineCommentPeekService> peekService;
        Lazy<IPullRequestSessionManager> sessionManager;
        Lazy<IPackageSettings> packageSettings;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            IGitHubServiceProvider serviceProvider,
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory)
        {
            this.serviceProvider = serviceProvider;
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = new Lazy<IInlineCommentPeekService>(() => serviceProvider.TryGetService<IInlineCommentPeekService>());
            this.sessionManager = new Lazy<IPullRequestSessionManager>(() => serviceProvider.TryGetService<IPullRequestSessionManager>());
            this.packageSettings = new Lazy<IPackageSettings>(() => serviceProvider.TryGetService<IPackageSettings>());
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            if (IsMarginDisabled(wpfTextViewHost))
            {
                return null;
            }

            var textView = wpfTextViewHost.TextView;
            var glyphFactory = new InlineCommentGlyphFactory(peekService.Value, textView);

            Func<Grid> gridFactory = () => new GlyphMarginGrid();
            var editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);
            return CreateMargin(glyphFactory, gridFactory, wpfTextViewHost, parent, editorFormatMap);
        }

        IWpfTextViewMargin CreateMargin<TGlyphTag>(IGlyphFactory<TGlyphTag> glyphFactory, Func<Grid> gridFactory,
            IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent, IEditorFormatMap editorFormatMap) where TGlyphTag : ITag
        {
            var tagAggregator = tagAggregatorFactory.CreateTagAggregator<TGlyphTag>(wpfTextViewHost.TextView);
            var margin = new GlyphMargin<TGlyphTag>(wpfTextViewHost, glyphFactory, gridFactory, tagAggregator, editorFormatMap,
                IsMarginVisible, MarginPropertiesName, MarginName, true, 17.0);

            if(IsDiffView(wpfTextViewHost))
            {
                TrackCommentGlyph(wpfTextViewHost, margin.VisualElement);
            }

            return margin;
        }

        bool IsMarginDisabled(IWpfTextViewHost textViewHost) => !packageSettings.Value.EditorComments && !IsDiffView(textViewHost);

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
