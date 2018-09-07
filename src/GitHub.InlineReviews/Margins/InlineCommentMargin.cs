using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.Extensions;
using GitHub.InlineReviews.Tags;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.Glyph;
using GitHub.InlineReviews.Services;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using ReactiveUI;

namespace GitHub.InlineReviews.Margins
{
    public sealed class InlineCommentMargin : IWpfTextViewMargin
    {
        public const string MarginName = "InlineComment";
        const string MarginPropertiesName = "Indicator Margin"; // Same background color as Glyph margin 

        readonly IWpfTextView textView;
        readonly IPullRequestSessionManager sessionManager;
        readonly Grid marginGrid;

        GlyphMargin<InlineReviewTag> glyphMargin;
        IDisposable currentSessionSubscription;
        IDisposable visibleSubscription;
        bool hasChanges;
        bool hasInfo;

        public InlineCommentMargin(
            IWpfTextViewHost wpfTextViewHost,
            IInlineCommentPeekService peekService,
            IEditorFormatMapService editorFormatMapService,
            IViewTagAggregatorFactoryService tagAggregatorFactory,
            Lazy<IPullRequestSessionManager> sessionManager)
        {
            textView = wpfTextViewHost.TextView;
            this.sessionManager = sessionManager.Value;

            // Default to not show comment margin
            textView.Options.SetOptionValue(InlineCommentTextViewOptions.MarginEnabledId, false);

            marginGrid = new GlyphMarginGrid { Width = 17.0 };
            var glyphFactory = new InlineGlyphFactory(peekService, textView);
            var editorFormatMap = editorFormatMapService.GetEditorFormatMap(textView);

            glyphMargin = new GlyphMargin<InlineReviewTag>(textView, glyphFactory, marginGrid, tagAggregatorFactory,
                editorFormatMap, MarginPropertiesName);

            if (IsDiffView())
            {
                TrackCommentGlyph(wpfTextViewHost, marginGrid);
            }

            currentSessionSubscription = this.sessionManager.WhenAnyValue(x => x.CurrentSession)
                .Subscribe(x => RefreshCurrentSession().Forget());

            visibleSubscription = marginGrid.WhenAnyValue(x => x.IsVisible)
                .Subscribe(x => textView.Options.SetOptionValue(InlineCommentTextViewOptions.MarginVisibleId, x));

            textView.Options.OptionChanged += (s, e) => RefreshMarginVisibility();
        }

        async Task RefreshCurrentSession()
        {
            var sessionFile = await FindSessionFile();
            hasChanges = sessionFile?.Diff != null && sessionFile.Diff.Count > 0;

            await Task.Yield(); // HACK: Give diff view a chance to initialize.
            var info = sessionManager.GetTextBufferInfo(textView.TextBuffer);
            hasInfo = info != null;

            RefreshMarginVisibility();
        }

        public ITextViewMargin GetTextViewMargin(string name)
        {
            return (name == MarginName) ? this : null;
        }

        public void Dispose()
        {
            visibleSubscription?.Dispose();
            visibleSubscription = null;

            currentSessionSubscription?.Dispose();
            currentSessionSubscription = null;

            glyphMargin?.Dispose();
            glyphMargin = null;
        }

        public FrameworkElement VisualElement => marginGrid;

        public double MarginSize => marginGrid.Width;

        public bool Enabled => IsMarginVisible();

        async Task<IPullRequestSessionFile> FindSessionFile()
        {
            await sessionManager.EnsureInitialized();

            var session = sessionManager.CurrentSession;
            if (session == null)
            {
                return null;
            }

            var relativePath = sessionManager.GetRelativePath(textView.TextBuffer);
            if (relativePath == null)
            {
                return null;
            }

            return await session.GetFile(relativePath);
        }

        bool IsDiffView() => textView.Roles.Contains("DIFF");

        void TrackCommentGlyph(IWpfTextViewHost host, UIElement marginElement)
        {
            var router = new MouseEnterAndLeaveEventRouter<AddInlineCommentGlyph>();
            router.Add(host.HostControl, marginElement);
        }

        void RefreshMarginVisibility()
        {
            marginGrid.Visibility = IsMarginVisible() ? Visibility.Visible : Visibility.Collapsed;
        }

        bool IsMarginVisible()
        {
            var enabled = textView.Options.GetOptionValue(InlineCommentTextViewOptions.MarginEnabledId);
            return hasInfo || (enabled && hasChanges);
        }
    }
}
