using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Language.Intellisense;
using GitHub.InlineReviews.Tags;
using GitHub.Factories;
using GitHub.Services;

namespace ScratchMargin
{
    internal class PullRequestReviewMargin : Grid, IWpfTextViewMargin
    {
        public const string MarginName = "PullRequestReview";
        public const string GlyphMarginName = "Glyph";

        IPullRequestSession session;

        readonly IWpfTextViewHost wpfTextViewHost;
        readonly MouseEnterAndLeaveEventRouter<AddInlineCommentGlyph> mouseEnterAndLeaveEventRouter;

        readonly InlineCommentGlyphMouseProcessor mouseProcessor;

        private bool isDisposed;

        IWpfTextViewMargin glyphMargin;

        public PullRequestReviewMargin(
            IWpfTextViewHost wpfTextViewHost,
            IPullRequestSessionManager sessionManager,
            IApiClientFactory apiClientFactory,
            IPeekBroker peekBroker,
            IViewTagAggregatorFactoryService tagAggregatorFactory)
        {
            this.wpfTextViewHost = wpfTextViewHost;
            wpfTextViewHost.HostControl.Loaded += HostControl_Loaded;

            mouseEnterAndLeaveEventRouter = new MouseEnterAndLeaveEventRouter<AddInlineCommentGlyph>();
            MouseMove += ScratchEditorMargin_MouseMove;
            MouseLeave += ScratchEditorMargin_MouseLeave;

            ClipToBounds = true;
            Background = new SolidColorBrush(Colors.LightBlue);

            mouseProcessor = new InlineCommentGlyphMouseProcessor(
                apiClientFactory,
                peekBroker,
                wpfTextViewHost.TextView,
                this,
                tagAggregatorFactory.CreateTagAggregator<InlineCommentTag>(wpfTextViewHost.TextView));
            MouseDown += PullRequestReviewMargin_MouseDown;

            sessionManager.CurrentSession.Subscribe(s =>
            {
                session = s;
            });
        }

        private void PullRequestReviewMargin_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mouseProcessor.PreprocessMouseLeftButtonUp(e);
        }

        void ScratchEditorMargin_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mouseEnterAndLeaveEventRouter.MouseMove(this, e);
        }

        void ScratchEditorMargin_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mouseEnterAndLeaveEventRouter.MouseLeave(this, e);
        }

        private void HostControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (session == null)
            {
                return;
            }

            glyphMargin = wpfTextViewHost.GetTextViewMargin(GlyphMarginName);
            if(glyphMargin == null || !glyphMargin.Enabled)
            {
                return;
            }

            Width = glyphMargin.MarginSize;

            var grid = glyphMargin.VisualElement as Grid;
            if(grid == null)
            {
                return;
            }

            foreach(var child in grid.Children)
            {
                var canvas = child as Canvas;
                if(canvas != null)
                {
                    new GlyphCanvasManager(canvas, grid, this);
                }
            }
        }

        class GlyphCanvasManager
        {
            readonly Canvas canvas;
            readonly Grid glyphGrid;
            readonly Grid marginGrid;

            internal GlyphCanvasManager(Canvas canvas, Grid glyphGrid, Grid marginGrid)
            {
                this.canvas = canvas;
                this.glyphGrid = glyphGrid;
                this.marginGrid = marginGrid;

                canvas.LayoutUpdated += Canvas_LayoutUpdated;
            }

            private void Canvas_LayoutUpdated(object sender, EventArgs e)
            {
                if(canvas.Parent != glyphGrid)
                {
                    return;
                }

                if (!IsOneOfOurs())
                {
                    return;
                }

                glyphGrid.Children.Remove(canvas);
                marginGrid.Children.Add(canvas);
            }

            bool IsOneOfOurs()
            {
                foreach(var child in canvas.Children)
                {
                    if(child is AddInlineCommentGlyph || child is ShowInlineCommentGlyph)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public FrameworkElement VisualElement
        {
            get
            {
                ThrowIfDisposed();
                return this;
            }
        }

        public double MarginSize
        {
            get
            {
                ThrowIfDisposed();
                return Width;
            }
        }

        public bool Enabled
        {
            get
            {
                ThrowIfDisposed();
                return true;
            }
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                GC.SuppressFinalize(this);
                isDisposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(MarginName);
            }
        }
    }
}
