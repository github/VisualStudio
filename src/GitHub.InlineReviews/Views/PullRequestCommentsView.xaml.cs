using System.Windows.Controls;
using GitHub.VisualStudio.UI.Helpers;

namespace GitHub.InlineReviews
{
    public partial class PullRequestCommentsView : UserControl
    {
        public PullRequestCommentsView()
        {
            this.InitializeComponent();
            PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
        }
    }
}