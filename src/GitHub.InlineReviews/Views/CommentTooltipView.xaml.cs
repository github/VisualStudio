using System;
using GitHub.UI;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Views
{
    public class GenericCommentTooltipView : ViewBase<ICommentTooltipViewModel, CommentTooltipView>
    { }

    public partial class CommentTooltipView : GenericCommentTooltipView
    {
        public CommentTooltipView()
        {
            InitializeComponent();

            CommentScrollViewer.LayoutUpdated += WatchForScrollBarVisible;
        }

        void WatchForScrollBarVisible(object sender, EventArgs e)
        {
            if (CommentScrollViewer.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible)
            {
                CommentScrollViewer.ScrollToBottom();

                var messageFormat = InlineReviews.Resources.CommentTooltipStatusOverflowMessage;
                var message = string.Format(messageFormat, ViewModel.Comments.Count);
                StatusTextBlock.Text = message;
                CommentScrollViewer.LayoutUpdated -= WatchForScrollBarVisible;
            }
        }
    }
}
