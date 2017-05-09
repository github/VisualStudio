using System;
using System.Windows.Controls;
using GitHub.VisualStudio.UI.Helpers;

namespace GitHub.InlineReviews.Views
{
    public partial class CommentThreadView : UserControl
    {
        public CommentThreadView()
        {
            InitializeComponent();
            PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
        }
    }
}
