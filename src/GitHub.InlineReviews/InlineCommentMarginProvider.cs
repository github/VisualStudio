using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using GitHub.InlineReviews.Services;
using GitHub.Services;
using GitHub.Settings;

namespace GitHub.InlineReviews
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(InlineCommentMargin.MarginName)]
    [Order(After = PredefinedMarginNames.Glyph)]
    [MarginContainer(PredefinedMarginNames.Left)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class InlineCommentMarginProvider : IWpfTextViewMarginProvider
    {
        readonly IGitHubServiceProvider serviceProvider;
        readonly IEditorFormatMapService editorFormatMapService;
        readonly IViewTagAggregatorFactoryService tagAggregatorFactory;
        readonly IInlineCommentPeekService peekService;
        readonly IPackageSettings packageSettings;
        readonly Lazy<IPullRequestSessionManager> sessionManager;

        [ImportingConstructor]
        public InlineCommentMarginProvider(
            IGitHubServiceProvider serviceProvider,
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            IInlineCommentPeekService peekService,
            IPackageSettings packageSettings)
        {
            this.serviceProvider = serviceProvider;
            this.editorFormatMapService = editorFormatMapService;
            this.tagAggregatorFactory = tagAggregatorFactory;
            this.peekService = peekService;
            this.packageSettings = packageSettings;
            sessionManager = new Lazy<IPullRequestSessionManager>(() => serviceProvider.GetService<IPullRequestSessionManager>());
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin parent)
        {
            var margin = new InlineCommentMargin(
                wpfTextViewHost, peekService, editorFormatMapService, tagAggregatorFactory, packageSettings, sessionManager);

            if (margin.IsMarginDisabled(wpfTextViewHost))
            {
                return null;
            }

            return margin;
        }
    }
}
