using System;
using GitHub.InlineReviews.ViewModels;
using System.Windows.Controls;
using System.Globalization;

namespace GitHub.InlineReviews.Views
{
    public partial class TooltipCommentThreadView : UserControl
    {
        public TooltipCommentThreadView()
        {
            InitializeComponent();

            CommentScrollViewer.LayoutUpdated += WatchForScrollBarVisible;
        }

        void WatchForScrollBarVisible(object sender, EventArgs e)
        {
            if (CommentScrollViewer.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible)
            {
                CommentScrollViewer.ScrollToBottom();

                var viewModel = (ICommentThreadViewModel)DataContext;
                var messageFormat = InlineReviews.Resources.CommentTooltipStatusOverflowMessage;
                var message = string.Format(CultureInfo.CurrentCulture, messageFormat, viewModel.Comments.Count);
                StatusTextBlock.Text = message;
                CommentScrollViewer.LayoutUpdated -= WatchForScrollBarVisible;
            }
        }
    }
}
