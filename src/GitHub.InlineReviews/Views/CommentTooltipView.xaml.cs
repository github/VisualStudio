using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace GitHub.InlineReviews.Views
{
    public partial class CommentTooltipView : UserControl
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
                StatusTextBlock.Text = "Click to view more or reply";
                CommentScrollViewer.LayoutUpdated -= WatchForScrollBarVisible;
            }
        }
    }
}
