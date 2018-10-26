using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Documents;
using GitHub.VisualStudio.UI.Helpers;

namespace GitHub.VisualStudio.UI.Views.Documents
{
    [ExportViewFor(typeof(IPullRequestPageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestPageView : UserControl
    {
        public PullRequestPageView()
        {
            InitializeComponent();

            bodyMarkdown.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
        }
    }
}
