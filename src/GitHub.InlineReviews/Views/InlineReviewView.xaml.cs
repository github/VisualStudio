using System;
using System.Windows.Controls;
using GitHub.VisualStudio.UI.Helpers;

namespace GitHub.InlineReviews.Views
{
    public partial class InlineReviewView : UserControl
    {
        public InlineReviewView()
        {
            InitializeComponent();
            PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
        }
    }
}
